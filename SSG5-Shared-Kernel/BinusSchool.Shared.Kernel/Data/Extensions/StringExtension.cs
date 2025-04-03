using Newtonsoft.Json;

namespace BinusSchool.Data.Extensions
{
    public static class StringExtension
    {
        public static bool TryParseJson<T>(this string @this, out T result)
        {
            var isSuccess = true;
            var settings = new JsonSerializerSettings
            {
                Error = (sender, args) => { isSuccess = false; args.ErrorContext.Handled = true; },
                MissingMemberHandling = MissingMemberHandling.Error
            };
            result = JsonConvert.DeserializeObject<T>(@this, settings);
            
            return isSuccess;
        }
    }
}