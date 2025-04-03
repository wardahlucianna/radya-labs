using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json.Linq;
using shortid;
using shortid.Configuration;

namespace BinusSchool.Data.HttpTools
{
    public class HttpLoggingHandler : DelegatingHandler
    {
        private static readonly Lazy<string[]> _types = new Lazy<string[]>(() => new[] { "html", "text", "xml", "json", "txt" });
        
        private readonly ILogger<HttpLoggingHandler> _logger;

        public HttpLoggingHandler(ILogger<HttpLoggingHandler> logger)
        {
            _logger = logger ?? NullLogger<HttpLoggingHandler>.Instance;
        }

        public HttpLoggingHandler(HttpMessageHandler innerHandler = null, ILogger<HttpLoggingHandler> logger =  null) :
            base(innerHandler ?? new HttpClientHandler())
        {
            _logger = logger ?? NullLogger<HttpLoggingHandler>.Instance;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string id = default, msg = default;
            DateTime start = default, end = default;

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                id = ShortId.Generate(new GenerationOptions(length: 8, useSpecialCharacters: true));
                msg = $"[{id} Request]";

                _logger.LogDebug("{0} ========Start==========", msg);
                _logger.LogDebug("{0} {1} {2} {3}/{4}", msg, request.Method, request.RequestUri.PathAndQuery, request.RequestUri.Scheme, request.Version);
                _logger.LogDebug("{0} Host: {1}://{2}:{3}", msg, request.RequestUri.Scheme, request.RequestUri.Host, request.RequestUri.Port);

                foreach (var header in request.Headers)
                {
                    _logger.LogDebug("{0} {1}: {2}", msg, header.Key, string.Join(", ", header.Value));
                }

                if (request.Content != null)
                {
                    foreach (var header in request.Content.Headers)
                    {
                        _logger.LogDebug("{0} {1}: {2}", msg, header.Key, string.Join(", ", header.Value));
                    }

                    if (request.Content is StringContent || IsTextBasedContentType(request.Headers) || IsTextBasedContentType(request.Content.Headers))
                    {
                        var result = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var needToMask = request.RequestUri.PathAndQuery.Contains("/auth/up");
                        
                        if (needToMask)
                        {
                            var json = JObject.Parse(result);
                            if (json.TryGetValue("password", out var pwd))
                            {
                                var pwdLength = pwd.ToObject<string>()?.Length ?? 0;
                                if (pwdLength != 0)
                                    json["password"] = string.Join(string.Empty, Enumerable.Range(0, pwdLength).Select(_ => '*'));
                                
                                result = json.ToString();
                            }
                        }

                        _logger.LogDebug("{0} Content:", msg);
                        _logger.LogDebug("{0} {1}", msg, result);
                    }
                }

                start = DateTime.Now;
            }

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                end = DateTime.Now;
                
                _logger.LogDebug("{0} Duration: {1}", msg, end - start);
                _logger.LogDebug("{0} ==========End==========", msg);

                msg = $"[{id} Response]";
                _logger.LogDebug("{0} =========Start=========", msg);

                var resp = response;

                _logger.LogDebug("{0} {1}/{2} {3} {4}", msg, request.RequestUri.Scheme.ToUpper(), resp.Version, (int)resp.StatusCode, resp.ReasonPhrase);

                foreach (var header in resp.Headers)
                {
                    _logger.LogDebug("{0} {1}: {2}", msg, header.Key, string.Join(", ", header.Value));
                }

                if (resp.Content != null)
                {
                    foreach (var header in resp.Content.Headers)
                    {
                        _logger.LogDebug("{0} {1}: {2}", msg, header.Key, string.Join(", ", header.Value));
                    }

                    if (resp.Content is StringContent || IsTextBasedContentType(resp.Headers) || IsTextBasedContentType(resp.Content.Headers))
                    {
                        start = DateTime.Now;
                        var result = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        end = DateTime.Now;

                        _logger.LogDebug("{0} Content:", msg);
                        _logger.LogDebug("{0} {1}", msg, string.Join(string.Empty, result.Cast<char>()));
                        _logger.LogDebug("{0} Duration: {1}", msg, end - start);
                    }
                }
                _logger.LogDebug("{0} ==========End==========", msg);
            }

            return response;
        }

        private bool IsTextBasedContentType(HttpHeaders headers)
        {
            if (headers is null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            if (!headers.TryGetValues("Content-Type", out var values))
            {
                return false;
            }

            var header = string.Join(" ", values).ToLowerInvariant();

            return _types.Value.Any(header.Contains);
        }
    }
}
