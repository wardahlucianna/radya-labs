using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BinusSchool.Common.Abstractions
{
    public interface IRedisCache
    {
        Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
        Task DeleteAsync<T>(string key, CancellationToken cancellationToken = default);
        Task<bool> SetContainsAsync<T>(string key, T value, CancellationToken cancellationToken = default);
        Task<List<string>> GetKeysAsync(string pattern, CancellationToken cancellationToken = default);
        Task<List<T>> GetListByPatternAsync<T>(string key, CancellationToken cancellationToken = default);
        Task SetListAsync<T>(string key, List<T> value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    }
}
