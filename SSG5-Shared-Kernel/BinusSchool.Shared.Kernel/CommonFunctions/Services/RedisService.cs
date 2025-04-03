using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

namespace BinusSchool.Common.Functions.Services
{
    public class RedisService : IRedisCache
    {
        private readonly IConfiguration _configuration;
        private string _env { get; }

        private static readonly HashSet<Type> _primitiveTypes = new HashSet<Type>
        {
            typeof(string),
            typeof(char),
            typeof(int),
            typeof(long),
            typeof(Guid),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(short),
            typeof(uint),
            typeof(ulong)
        };

        private IDatabase _database { get; set; }
        private IConnectionMultiplexer _connectionMultiplexer { get; set; }

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        private readonly ILogger<RedisService> _logger;

        public RedisService(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<RedisService>();

            _env = "Development".ToUpper();

            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            if (!string.IsNullOrWhiteSpace(envName) && envName.ToUpper() != _env)
            {
                _env = envName.ToUpper();
            }
        }

        public string GetPrefixKey()
        {
            return _env switch
            {
                "STAGING" => "STAG-",
                "PRODUCTION" => "PROD-",
                _ => "DEV-"
            };
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return default;
            }

            key = GetPrefixKey() + key;

            try
            {
                await ConnectAsync(cancellationToken);
                var value = await _database!.StringGetAsync(key);
                var data = string.IsNullOrWhiteSpace(value) ? default : JsonConvert.DeserializeObject<T>(value);

                return data;
            }
            catch
            {
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null,
            CancellationToken cancellationToken = default)
        {
            key = GetPrefixKey() + key;

            try
            {
                await ConnectAsync(cancellationToken);

                var x = JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                await _database!.StringSetAsync(key, x, expiry);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurs when try to insert redis, message: {e?.Message}");
            }
        }

        public async Task DeleteAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            key = GetPrefixKey() + key;
            try
            {
                await ConnectAsync(cancellationToken);
                await _database!.KeyDeleteAsync(key);
            }
            catch
            {
                //
            }
        }

        public async Task<bool> SetContainsAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        {
            key = GetPrefixKey() + key;

            try
            {
                await ConnectAsync(cancellationToken);
                return await _database!.SetContainsAsync(key, AsString(value));
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<string>> GetKeysAsync(string pattern, CancellationToken cancellationToken = default)
        {
            pattern = GetPrefixKey() + pattern;

            var redisValues = new List<string>();
            try
            {
                await ConnectAsync(cancellationToken);
                var endpoints = _database.Multiplexer.GetEndPoints();
                redisValues = (await _database.Multiplexer.GetServer(endpoints.FirstOrDefault()).KeysAsync(pattern: pattern).ToListAsync(cancellationToken))
                    .Select(s => s.ToString()).ToList();

                return redisValues;
            }
            catch
            {
                return redisValues;
            }
        }

        public async Task<List<T>> GetListByPatternAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default;
            }

            var data = new List<T>();

            try
            {
                await ConnectAsync(cancellationToken);
                var keys = (await GetKeysAsync($"{key}*", cancellationToken));

                keys = keys.Select(e =>
                {
                    var split = e.Split("page:");

                    var page = 0;

                    if (split.Length > 1)
                        int.TryParse(split[1], out page);

                    return new { Keys = e, Page = page };
                })
                    .OrderBy(e => e.Page)
                    .Select(e => e.Keys)
                    .ToList();

                foreach (var myKey in keys)
                {
                    var value = await _database!.StringGetAsync(myKey);
                    var valueObject = string.IsNullOrWhiteSpace(value) ? default : JsonConvert.DeserializeObject<List<T>>(value);

                    data.AddRange(valueObject);
                }

                return data.Any() ? data : default;
            }
            catch
            {
                return default;
            }
        }

        public async Task SetListAsync<T>(string key, List<T> value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            key = GetPrefixKey() + key;

            try
            {
                await ConnectAsync(cancellationToken);

                var valueCunked = value.ChunkBy(5000);

                foreach (var (item, i) in valueCunked.Select((value, i) => (value, i)))
                {
                    var keyPage = $"{key}-page:{i + 1}";

                    var x = JsonConvert.SerializeObject(item, Formatting.Indented, new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    });

                    await _database!.StringSetAsync(keyPage, x, expiry);
                }
            }
            catch
            {
                // do nothing
            }
        }

        private static string AsString<T>(T value)
            => (_primitiveTypes.Contains(typeof(T)) ? value!.ToString() : JsonConvert.SerializeObject(value))!;

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (_database != null)
                return;

            await _connectionLock.WaitAsync(cancellationToken);

            try
            {
                if (_database == null)
                {
                    var connection =
                        await ConnectionMultiplexer.ConnectAsync(_configuration.GetConnectionString("Redis"));

                    _connectionMultiplexer = connection;
                    _database = connection.GetDatabase();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurs when try to connect redis");
            }
            finally
            {
                _connectionLock.Release();
            }
        }
    }
}
