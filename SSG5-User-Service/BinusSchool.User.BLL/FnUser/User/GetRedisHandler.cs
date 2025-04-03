using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;

namespace BinusSchool.User.FnUser.User
{
    public class GetRedisHandler : FunctionsHttpSingleHandler
    {
        private readonly IRedisCache _redisCache;

        public GetRedisHandler(IRedisCache redisCache)
        {
            _redisCache = redisCache;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var s = await _redisCache.GetAsync<string>("test-key");
            return Request.CreateApiResult2(new { value = s } as object);
        }
    }
}
