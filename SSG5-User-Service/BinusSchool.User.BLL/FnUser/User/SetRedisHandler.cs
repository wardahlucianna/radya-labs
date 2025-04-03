using System;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;

namespace BinusSchool.User.FnUser.User
{
    public class SetRedisHandler : FunctionsHttpSingleHandler
    {
        private readonly IRedisCache _redisCache;

        public SetRedisHandler(IRedisCache redisCache)
        {
            _redisCache = redisCache;
        }
        
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            await _redisCache.SetAsync("test-key", Guid.NewGuid().ToString());
            
            return Request.CreateApiResult2();
        }
    }
}
