using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.EmergencyAttendance
{
    public class EmergencyAttendanceEndpoint
    {
        private const string _route = "emergency-attendance";
        private const string _tag = "Emergency Attendance";

        [FunctionName(nameof(EmergencyAttendanceEndpoint.GetSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary per Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryRequest.Date), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SummaryResult))]
        public Task<IActionResult> GetSummary(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/summary")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EmergencyAttendanceEndpoint.GetUnsubmittedStudents))]
        [OpenApiOperation(tags: _tag, Summary = "Get Unsubmitted Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.Date), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UnsubmittedStudentResult))]
        public Task<IActionResult> GetUnsubmittedStudents(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unsubmitted")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUnsubmittedStudentsHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EmergencyAttendanceEndpoint.GetSubmittedStudents))]
        [OpenApiOperation(tags: _tag, Summary = "Get Submitted Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.Date), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnsubmittedStudentsRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSubmittedStudentsRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SubmittedStudentResult))]
        public Task<IActionResult> GetSubmittedStudents(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/submitted")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubmittedStudentsHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(EmergencyAttendanceEndpoint.Submit))]
        [OpenApiOperation(tags: _tag, Summary = "Submit Emergency Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SubmitRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> Submit(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/submit")] HttpRequest req,
            [Queue("notification-atd-student")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SubmitHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(EmergencyAttendanceEndpoint.Unsubmit))]
        [OpenApiOperation(tags: _tag, Summary = "Unsubmit Emergency Attendance")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UnsubmitRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> Unsubmit(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/unsubmit")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UnsubmitHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
