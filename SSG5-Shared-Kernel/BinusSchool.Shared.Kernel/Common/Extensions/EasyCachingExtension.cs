using EasyCaching.Core;

namespace BinusSchool.Common.Extensions
{
    public static class EasyCachingExtension
    {
        public static bool TryGetCacheValue<T>(this IEasyCachingProvider cachingProvider, string key, out T cacheValue)
        {
            cacheValue = default;
            var cacheResult = cachingProvider.Get<T>(key);

            if (cacheResult.HasValue)
            {
                cacheValue = cacheResult.Value;
                return true;
            }
            
            return false;
        }
    }
}
