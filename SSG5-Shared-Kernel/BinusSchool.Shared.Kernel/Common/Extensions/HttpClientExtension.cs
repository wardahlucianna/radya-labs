using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BinusSchool.Common.Extensions
{
    public static class HttpClientExtension
    {
        public static async Task<string> GetImageAsBase64(this HttpClient client, string url)
        {
            var bytes = await client.GetByteArrayAsync(url);
            return Convert.ToBase64String(bytes);
        }
    }
}
