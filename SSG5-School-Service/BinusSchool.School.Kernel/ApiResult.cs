using System.Net;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace BinusSchool.School.Kernel;

internal class ApiResult<T> : ApiResult
{
    /// <summary>
    /// Data payload response
    /// </summary>
    public T Payload { get; set; }
}

internal class ApiResult
{
    /// <summary>
    /// Inner message to user if error exception. Default value is null
    /// </summary>

    public string InnerMessage { get; set; }

    /// <summary>
    /// Collection of field & descriptions that caused errors
    /// </summary>
    public IDictionary<string, IEnumerable<string>> Errors { get; set; } =
        new Dictionary<string, IEnumerable<string>>();

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
    public string Env =>
        Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT", EnvironmentVariableTarget.Process);

    /// <summary>
    /// API Message
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Additional response properties
    /// </summary>
    public IDictionary<string, object> Properties { get; set; }
}

public static class ApiResultExt
{
    public static OkObjectResult Create<T>(T value,
        string path,
        IDictionary<string, object> props = null,
        string message = "OK",
        string innerMessage = null,
        bool isSuccess = true,
        HttpStatusCode code = HttpStatusCode.OK,
        IDictionary<string, IEnumerable<string>> errors = null)
        => new(new ApiResult<T>
        {
            IsSuccess = isSuccess,
            StatusCode = (int)code,
            Path = path,
            Message = message,
            InnerMessage = innerMessage,
            Properties = props,
            Payload = value,
            Errors = errors
        });

    public static OkObjectResult CreateError(ValidationResult result, string path)
    {
        var apiResult = new ApiResult
        {
            Path = path,
            Message = "Invalid parameter",
            StatusCode = (int)HttpStatusCode.BadRequest
        };

        var dictionary = result.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(e => e.Key, e => e.ToList());
        foreach (var item in dictionary)
        {
            var list = new List<string>(item.Value.Count);
            foreach (var item2 in item.Value)
                list.Add(item2.ErrorMessage);
            apiResult.Errors.Add(item.Key, list);
        }

        return new OkObjectResult(apiResult);
    }

    public static OkObjectResult CreateError(string path, string message)
    {
        var apiResult = new ApiResult
        {
            Path = path,
            Message = message,
            StatusCode = (int)HttpStatusCode.BadRequest
        };

        return new OkObjectResult(apiResult);
    }
}