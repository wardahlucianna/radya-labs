using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;

namespace BinusSchool.Data.Extensions
{
    public static class ApiResultExtension
    {
        public static Task<ApiErrorResult<T>> DoNotThrowException<T>(this Task<ApiErrorResult<T>> task) where T : class
        {
            try
            {
                return task;
            }
            catch (Exception ex)
            {
                var result = new ApiErrorResult<T>
                {
                    InnerMessage = ex.InnerException?.Message
                };
                (result.StatusCode, result.Message, result.Errors) = ex switch
                {
                    ApiException api => ((int)api.StatusCode, api.ReasonPhrase ?? api.Message, GetErrors(api.Content)),
                    _ => ((int)HttpStatusCode.InternalServerError, ex.Message, default(IDictionary<string, IEnumerable<string>>))
                };

                return Task.FromResult(result);
            }
        }

        private static IDictionary<string, IEnumerable<string>> GetErrors(string jsonContent)
        {
            if (string.IsNullOrWhiteSpace(jsonContent))
                return null;
            else if (jsonContent.TryParseJson<IDictionary<string, IEnumerable<string>>>(out var jsonObjectList))
                return jsonObjectList;
            else if (jsonContent.TryParseJson<IDictionary<string, string>>(out var jsonObject))
                return jsonObject.ToDictionary(x => x.Key, x => new[] { x.Value }.AsEnumerable());
            else
                return null;
        }
    }
}