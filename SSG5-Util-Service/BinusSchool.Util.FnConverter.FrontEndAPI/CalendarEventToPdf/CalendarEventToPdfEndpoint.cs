using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Util.FnConverter.CalendarEventToPdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Util.FnConverter.CalendarEventToPdf
{
    public class CalendarEventToPdfEndpoint
    {
        private const string _route = "calendar-event-to-pdf";
        private const string _tag = "Convert Calendar Event to Pdf";

        [FunctionName(nameof(CalendarEventToPdfEndpoint.ConvertCalendarEventToPdf))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(ConvertCalendarEventToPdfRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ConvertCalendarEventToPdfRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ConvertCalendarEventToPdfRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.OK)]
        public Task<IActionResult> ConvertCalendarEventToPdf(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ConvertCalendarEventToPdfHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}
