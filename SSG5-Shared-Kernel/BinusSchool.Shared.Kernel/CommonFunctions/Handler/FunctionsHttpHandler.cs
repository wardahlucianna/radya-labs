using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Common.Functions.Handler
{
    public abstract class FunctionsHttpHandler
    {
        protected AuthenticationInfo AuthInfo;
        protected HttpRequest Request;
        protected CancellationToken CancellationToken;
        protected IStringLocalizer Localizer;
        protected ILogger<FunctionsHttpHandler> Logger;
        protected IDictionary<string, object> KeyValues;
        protected IConfiguration Configuration;
        protected BinusSchoolApiConfiguration2 ApiConfiguration => Configuration?.GetSection("BinusSchoolService")?.Get<BinusSchoolApiConfiguration2>();

        /// <summary>
        /// Fill <see cref="Configuration"/> and <see cref="ApiConfiguration"/> from Azure App Configuration.
        /// Call this before invoke Binus School api
        /// </summary>
        protected void FillConfiguration()
        {
            Configuration = Request.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        }

        /// <summary>
        /// User validation with Authorization header.
        /// </summary>
        /// <returns>User authenticated information</returns>
        protected virtual AuthenticationInfo ValidateUser()
        {
            return Request.EnsureAnyUser();
        }

        /// <summary>
        /// Exception block of try catch.
        /// </summary>
        /// <param name="ex">Exception</param>
        protected virtual Task<IActionResult> OnException(Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            var response = Request.CreateApiErrorResponse(ex);
            
            return Task.FromResult(response as IActionResult);
        }

        /// <summary>
        /// Finally block of try catch.
        /// </summary>
        protected virtual void OnFinally() { }
        
        protected Task<IEnumerable<string>> GetIdsFromBody()
        {
            return Request.GetBody<IEnumerable<string>>();
        }

        protected ApiErrorResult<object> ProceedDeleteResult(ApiErrorResult<object> result)
        {
            if (result.Errors != null && result.Errors.Count != 0)
            {
                result.IsSuccess = false;
                result.Message = Localizer["ExCantDelete"];
                result.StatusCode = StatusCodes.Status400BadRequest;
            }

            return result;
        }

        protected ApiErrorResult<object> ProceedDeleteResult(IDictionary<string, IEnumerable<string>> errors)
        {
            var result = Request.CreateApiResult2(errors: errors);

            return ProceedDeleteResult(result);
        }
    }
}
