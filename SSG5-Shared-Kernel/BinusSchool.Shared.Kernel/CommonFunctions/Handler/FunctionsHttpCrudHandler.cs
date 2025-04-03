using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.I18n.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Common.Functions.Handler
{
    public abstract class FunctionsHttpCrudHandler : FunctionsHttpHandler, IFunctionsHttpHandler
    {
        protected IDbContextTransaction Transaction;

        public virtual async Task<IActionResult> Execute(HttpRequest request, CancellationToken cancellationToken, bool validateUser = true, params KeyValuePair<string, object>[] keyValues)
        {
            try
            {
                await request.HttpContext.DetermineLocalization();

                Request = request;
                CancellationToken = cancellationToken;
                Localizer = request.HttpContext.RequestServices.GetService<IStringLocalizer>();
                Logger = request.HttpContext.RequestServices.GetService<ILogger<FunctionsHttpCrudHandler>>();
                KeyValues = keyValues.ToDictionary(x => x.Key, x => x.Value);

                if (validateUser)
                    AuthInfo = ValidateUser();
                
                // refresh configuration asynchronously without blocking the execution of the current function
                var refresherProvider = request.HttpContext.RequestServices.GetRequiredService<IConfigurationRefresherProvider>();
                _ = refresherProvider.Refreshers.First().TryRefreshAsync();

                var result = default(object);

                // POST
                if (HttpMethods.IsPost(request.Method))
                    result = await PostHandler();
                
                // PUT
                else if (HttpMethods.IsPut(request.Method))
                    result = await PutHandler();
                
                // DELETE
                else if (HttpMethods.IsDelete(request.Method))
                {
                    var ids = (await GetIdsFromBody()).Distinct();
                    var deleted = await DeleteHandler(ids);

                    result = ProceedDeleteResult(deleted);
                }

                // GET
                else if (KeyValues.TryGetValue("id", out var id))
                {
                    var detail = await GetDetailHandler(id as string);
                    if (detail.Payload is null)
                        throw new NotFoundException(string.Format(Localizer["ExNotExist"], Localizer["Resource"], "Id", id));

                    result = detail;
                }
                else
                {
                    result = await GetHandler();           
                }

                return new JsonResult(result, SerializerSetting.GetJsonSerializer(request.IsShowAll()));
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

        protected abstract Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler();
        protected abstract Task<ApiErrorResult<object>> GetDetailHandler(string id);
        protected abstract Task<ApiErrorResult<object>> PostHandler();
        protected abstract Task<ApiErrorResult<object>> PutHandler();
        protected abstract Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids);

        protected override Task<IActionResult> OnException(Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            Transaction?.Rollback();
            var response = Request.CreateApiErrorResponse(ex);

            return Task.FromResult(response as IActionResult);
        }

        protected override void OnFinally()
        {
            Transaction?.Dispose();
        }
    }
}
