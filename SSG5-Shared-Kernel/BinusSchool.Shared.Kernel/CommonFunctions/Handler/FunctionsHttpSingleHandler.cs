using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.I18n.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Common.Functions.Handler
{
    public abstract class FunctionsHttpSingleHandler : FunctionsHttpHandler, IFunctionsHttpHandler
    {
        public virtual async Task<IActionResult> Execute(HttpRequest request, CancellationToken cancellationToken, bool validateUser = true, params KeyValuePair<string, object>[] keyValues)
        {
            try
            {
                await request.HttpContext.DetermineLocalization();

                Request = request;
                CancellationToken = cancellationToken;
                Localizer = request.HttpContext.RequestServices.GetService<IStringLocalizer>();
                Logger = request.HttpContext.RequestServices.GetService<ILogger<FunctionsHttpSingleHandler>>();
                KeyValues = keyValues.ToDictionary(x => x.Key, x => x.Value);

                if (validateUser)
                    AuthInfo = ValidateUser();
                
                // refresh configuration asynchronously without blocking the execution of the current function
                var refresherProvider = request.HttpContext.RequestServices.GetRequiredService<IConfigurationRefresherProvider>();
                _ = refresherProvider.Refreshers.First().TryRefreshAsync();
                
                Logger.LogInformation("[QueryString] {0}", Request.QueryString.HasValue ? Request.QueryString.Value: "No query string");
                
                return await RawHandler();
            }
            catch (Exception ex)
            {
                return await OnException(ex);
            }
            finally
            {
                OnFinally();
            }
        }

        protected abstract Task<ApiErrorResult<object>> Handler();

        protected virtual async Task<IActionResult> RawHandler()
        {
            var result = await Handler();
            return new JsonResult(result, SerializerSetting.GetJsonSerializer(Request.IsShowAll()));
        }
    }
}
