using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Util.FnConverter.CalendarEventToPdf;
using BinusSchool.Util.FnConverter.CalendarEventToPdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Data.Model.Util.FnConverter.StudentAchivementToPdf;
using Microsoft.Extensions.DependencyInjection;

namespace BinusSchool.Util.FnConverter.StudentAchievementToPdf
{
    public class StudentAchievementToPdfEndpoint
    {
        private const string _route = "student-achievement-to-pdf";
        private const string _tag = "Student Achievement to Pdf";

        [FunctionName(nameof(ConvertStudentAchievementToPdf))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(StudentAchievementToPdfRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(StudentAchievementToPdfRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(StudentAchievementToPdfRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(StudentAchievementToPdfRequest.Type), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(StudentAchievementToPdfRequest.Role), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(StudentAchievementToPdfRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(StudentAchievementToPdfRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(StudentAchievementToPdfRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(StudentAchievementToPdfRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.OK)]
        public Task<IActionResult> ConvertStudentAchievementToPdf(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<StudentAchievementToPdfHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}
