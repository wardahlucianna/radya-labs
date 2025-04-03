using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Model;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace BinusSchool.Common.Extensions
{
    public static class HttpRequestExtension
    {
        private static readonly ConcurrentDictionary<string, PropertyInfo[]> _cachedProps = new ConcurrentDictionary<string, PropertyInfo[]>();
        private static readonly ConcurrentDictionary<string, CultureInfo> _cachedCultures = new ConcurrentDictionary<string, CultureInfo>();
        
        public static ApiErrorResult CreateApiResult2(this HttpRequest req, 
            IDictionary<string, object> props = null, 
            string message = "OK",
            string innerMessage = null,
            bool isSuccess = true, 
            HttpStatusCode code = HttpStatusCode.OK,
            IDictionary<string, IEnumerable<string>> errors = null)
        {
            return new ApiErrorResult
            {
                IsSuccess = isSuccess,
                StatusCode = (int)code,
                Path = req.Path.Value,
                Message = message,
                InnerMessage = innerMessage,
                Properties = props,
                Errors = errors
            };
        }

        public static ApiErrorResult<T> CreateApiResult2<T>(this HttpRequest req, 
            T value, 
            IDictionary<string, object> props = null, 
            string message = "OK", 
            string innerMessage = null, 
            bool isSuccess = true, 
            HttpStatusCode code = HttpStatusCode.OK,
            IDictionary<string, IEnumerable<string>> errors = null)
            where T : class
        {
            return new ApiErrorResult<T>
            {
                IsSuccess = isSuccess,
                StatusCode = (int)code,
                Path = req.Path.Value,
                Message = message,
                InnerMessage = innerMessage,
                Properties = props,
                Payload = value,
                Errors = errors
            };
        }
        
        public static JsonResult CreateApiResponse(this HttpRequest req, object value, IDictionary<string, object> props = null, string message = "OK", bool isSuccess = true, HttpStatusCode code = HttpStatusCode.OK)
        {
            return new JsonResult(new ApiResult
            {
                IsSuccess = isSuccess,
                StatusCode = (int)code,
                Path = req.Path.Value,
                Message = message,
                Properties = props,
                Payload = value
            },
            Utils.SerializerSetting.GetJsonSerializer(req.IsShowAll()));
        }

        public static JsonResult CreateApiErrorResponse(this HttpRequest req, Exception exception)
        {
            var result = new ApiErrorResult
            {
                Path = req.Path.Value,
                Message = exception.Message,
                InnerMessage = exception.InnerException?.Message
            };

            (result.StatusCode, result.Errors) = exception switch
            {
                ModelValidationException modelValidation => ((int)HttpStatusCode.BadRequest, modelValidation.Failures),
                BadRequestException _                    => ((int)HttpStatusCode.BadRequest, null),
                NotFoundException _                      => ((int)HttpStatusCode.NotFound, null),
                UnauthorizeException _                   => ((int)HttpStatusCode.Unauthorized, null),
                ForbiddenException _                     => ((int)HttpStatusCode.Forbidden, null),
                NotImplementedException _                => ((int)HttpStatusCode.NotImplemented, null),
                _                                        => ((int)HttpStatusCode.InternalServerError, null)
            };

            if (result.StatusCode == (int)HttpStatusCode.InternalServerError)
            {
                result.InnerMessage = "System error. Please contact your admin";
            }

            return new JsonResult(result, Utils.SerializerSetting.GetJsonSerializer(req.IsShowAll()));
        }

        public static bool IsShowAll(this HttpRequest req)
        {
            var isShowAll = true;
            if (req.Query.TryGetValue("showAll", out var showAll) && bool.TryParse(showAll, out isShowAll)) {}

            return isShowAll;
        }

        public static T GetParams<T>(this HttpRequest req) 
            where T : class, new()
        {
            return req.ValidateParams<T>(null);
        }

        public static T ValidateParams<T>(this HttpRequest req, params string[] requiredProps)
            where T : class, new()
        {
            var param = new T();
            var emptyProps = requiredProps != null ? new List<string>() : null;
            var modelName = typeof(T).FullName ?? typeof(T).Name;

            // cache model properties
            _cachedProps.TryAdd(modelName, typeof(T).GetProperties());

            foreach (var propertyInfo in _cachedProps[modelName])
            {
                if (req.Query.TryGetValue(propertyInfo.Name, out var valString) && !string.IsNullOrEmpty(valString.FirstOrDefault()))
                {
                    object valType;
                    var propType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

                    if (propType.IsEnum)
                    {
                        Enum.TryParse(propType, valString.First(), true, out valType);
                    }
                    else if (typeof(DateTime).IsAssignableFrom(propType))
                    {
                        valType = ConvertToDateTime(valString.First());
                    }
                    else if (propType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propType))
                    {
                        var collectionInstance = Activator.CreateInstance(typeof(List<>).MakeGenericType(propType.GenericTypeArguments[0])) as IList;
                        var typeConverter = TypeDescriptor.GetConverter(propType.GenericTypeArguments[0]);
                        
                        foreach (var item in valString.First().Split(','))
                        {
                            var value = typeof(DateTime).IsAssignableFrom(propType.GenericTypeArguments[0])
                                ? ConvertToDateTime(item)
                                : typeConverter.ConvertFromString(item);
                            collectionInstance!.Add(value);
                        }
                        
                        valType = collectionInstance;
                    }
                    else
                    {
                        valType = TypeDescriptor.GetConverter(propType).ConvertFromString(valString.First());
                    }
                    
                    propertyInfo.SetValue(param, valType);
                }
                else if (requiredProps != null)
                {
                    emptyProps.Add(propertyInfo.Name);
                }
            }

            if (requiredProps != null)
            {
                emptyProps = emptyProps.Intersect(requiredProps).ToList();
                if (emptyProps.Count != 0)
                {
                    var localizer = req.HttpContext.RequestServices.GetService<IStringLocalizer>();
                    throw new ModelValidationException(
                        localizer: localizer,
                        failures: emptyProps
                            .Select(x => KeyValuePair.Create(x, string.Format(localizer["ExEmptyParam"], x)))
                            .ToDictionary(x => x.Key, x => new[] { x.Value }.AsEnumerable()));
                }
            }

            DateTime ConvertToDateTime(string value)
            {
                var reqLang = req.GetUserLanguages().FirstOrDefault();
                if (reqLang != null)
                    _cachedCultures.TryAdd(reqLang, new CultureInfo(reqLang));
                
                return reqLang is null
                    ? DateTime.Parse(value)
                    : DateTime.Parse(value, _cachedCultures[reqLang]);
            }

            return param;
        }

        public static T ValidateParams<T, V>(this HttpRequest req)
            where T : class, new()
            where V : AbstractValidator<T>, new()
        {
            var param = req.ValidateParams<T>();
            var localizer = req.HttpContext.RequestServices.GetService<IStringLocalizer>();
            new V().Validate(param).EnsureValid(localizer: localizer);

            return param;
        }

        public static async Task<T> GetBody<T>(this HttpRequest req)
            where T : class
        {
            var rawBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            return JsonConvert.DeserializeObject<T>(rawBody);
        }

        public static async Task<T> ValidateBody<T, V>(this HttpRequest req)
            where T : class
            where V : AbstractValidator<T>, new()
        {
            var body = await req.GetBody<T>();
            var localizer = req.HttpContext.RequestServices.GetService<IStringLocalizer>();
            new V().Validate(body).EnsureValid(localizer: localizer);

            return body;
        }

        public static async Task<T> GetBodyForm<T>(this HttpRequest req)
           where T : class,new()
        {

            var param = new T();
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var propType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                var getBodyForm =await req.ReadFormAsync();
                if (getBodyForm.TryGetValue(propertyInfo.Name, out var valString) && !valString.Any(x => string.IsNullOrEmpty(x)))
                {
                    var valType = default(object);
                    if (propType.IsEnum)
                    {
                        Enum.TryParse(propType, valString.ToString(), true, out valType);
                    }
                    else if (propType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propType))
                    {
                        var collectionInstance = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(propType.GenericTypeArguments[0]));
                        var data = JsonConvert.DeserializeObject<List<object>>(valString);
                        foreach (var item in data)
                        {
                            collectionInstance.Add(Convert.ChangeType(item, propType.GenericTypeArguments[0]));

                        }
                        valType = collectionInstance;
                    }
                    //else if(typeof(bool).IsAssignableFrom(propType))
                    //{
                    //    valType = Convert.ToBoolean(valString[1]);
                    //}
                    else
                    {
                        valType = Convert.ChangeType(valString.ToString(), propType);
                    }

                    propertyInfo.SetValue(param, valType);
                }
            }

            return param;
        }

        public static async Task<T> ValidateBodyForm<T, V>(this HttpRequest req)
           where T : class , new()
           where V : AbstractValidator<T>, new()
        {
            var body = await req.GetBodyForm<T>();
            var localizer = req.HttpContext.RequestServices.GetService<IStringLocalizer>();
            new V().Validate(body).EnsureValid(localizer: localizer);

            return body;
        }

        public static string[] GetUserLanguages(this HttpRequest request)
        {
            return request.GetTypedHeaders()
                .AcceptLanguage?
                .OrderByDescending(x => x.Quality ?? 1)
                .Select(x => x.Value.ToString())
                .ToArray() ?? Array.Empty<string>();
        }
    }
}
