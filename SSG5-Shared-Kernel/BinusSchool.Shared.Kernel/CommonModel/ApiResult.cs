using System;
using System.Collections.Generic;

namespace BinusSchool.Common.Model
{
    public class ApiResult<T>
    {
        /// <summary>
        /// Flag to user response API success or not
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// HttpStatusCode and OtherStatusCode
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Application environment
        /// </summary>
        public string Env => Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT", EnvironmentVariableTarget.Process);

        /// <summary>
        /// API Message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Additional response properties
        /// </summary>
        public IDictionary<string, object> Properties { get; set; }

        /// <summary>
        /// Data payload response
        /// </summary>
        public T Payload { get; set; }
    }

    public class ApiResult : ApiResult<object> { }

    public class ApiErrorResult<T> : ApiResult<T>
    {
        /// <summary>
        /// Inner message to user if error exception. Default value is null
        /// </summary>

        public string InnerMessage { get; set; }
        /// <summary>
        /// Collection of field & descriptions that caused errors
        /// </summary>
        public IDictionary<string, IEnumerable<string>> Errors { get; set; }
    }

    public class ApiErrorResult : ApiErrorResult<object> { }
}