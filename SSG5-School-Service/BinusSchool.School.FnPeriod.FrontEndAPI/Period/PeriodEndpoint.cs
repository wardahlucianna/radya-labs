using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnPeriod.Period
{
    public class PeriodEndpoint
    {
        private const string _route = "school/period";
        private const string _tag = "School Period";

        private readonly PeriodHandler _handler;
        private readonly TermHandler _termHandler;
        private readonly GetDatePeriodWithGradeHandler _getDatePeriodWithGradeHandler;
        private readonly GetDateBySemesterHandler _getDateBySemesterHandler;
        private readonly CurrentAcademicYearHandler _currentAcademicYearHandler;
        private readonly GetPeriodByAcademicYearHandler _getPeriodByAcademicYearHandler;
        private readonly GetCurrentPeriodHandler _getCurrentPeriodHandler;
        public PeriodEndpoint(PeriodHandler handler, TermHandler termHandler, GetDatePeriodWithGradeHandler getDatePeriodWithGradeHandler, CurrentAcademicYearHandler currentAcademicYearHandler, GetDateBySemesterHandler getDateBySemesterHandler, GetPeriodByAcademicYearHandler getPeriodByAcademicYearHandler, GetCurrentPeriodHandler getCurrentPeriodHandler)
        {
            _handler = handler;
            _termHandler = termHandler;
            _getDatePeriodWithGradeHandler = getDatePeriodWithGradeHandler;
            _getDateBySemesterHandler = getDateBySemesterHandler;
            _currentAcademicYearHandler = currentAcademicYearHandler;
            _getPeriodByAcademicYearHandler = getPeriodByAcademicYearHandler;
            _getCurrentPeriodHandler = getCurrentPeriodHandler;
        }

        [FunctionName(nameof(PeriodEndpoint.GetPeriods))]
        [OpenApiOperation(tags: _tag, Summary = "Get Period List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPeriodRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPeriodRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPeriodRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetPeriodRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetPeriodRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPeriodRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPeriodRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetPeriodRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPeriodRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPeriodRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPeriodRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPeriodResult))]
        public Task<IActionResult> GetPeriods(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PeriodEndpoint.GetPeriodDate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Period start date and end date with grade ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPeriodRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPeriodResult))]
        public Task<IActionResult> GetPeriodDate(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"/get-date-with-grade")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getDatePeriodWithGradeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PeriodEndpoint.GetPeriodDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Period Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPeriodDetailResult))]
        public Task<IActionResult> GetPeriodDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(PeriodEndpoint.AddPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Add Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddPeriodRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(PeriodEndpoint.UpdatePeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Update Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdatePeriodRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdatePeriod(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PeriodEndpoint.DeletePeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeletePeriod(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PeriodEndpoint.GetTerm))]
        [OpenApiOperation(tags: _tag, Summary = "Get Term")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(SelectTermRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(SelectTermRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(SelectTermRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(SelectTermRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(SelectTermRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(SelectTermRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(SelectTermRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(SelectTermRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SelectTermResult))]
        public Task<IActionResult> GetTerm(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-semester")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _termHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PeriodEndpoint.GetCurrenctAcademicYear))]
        [OpenApiOperation(tags: _tag, Summary = "Get Current Academic Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CurrentAcademicYearRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CurrentAcademicYearResult))]
        public Task<IActionResult> GetCurrenctAcademicYear(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/current-academicyear")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _currentAcademicYearHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PeriodEndpoint.GetPeriodDateBySemester))]
        [OpenApiOperation(tags: _tag, Summary = "Get Period start date and end date with semester ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDateBySemesterRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDateBySemesterRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDateBySemesterResult))]
        public Task<IActionResult> GetPeriodDateBySemester(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-date-with-semester")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getDateBySemesterHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PeriodEndpoint.GetPeriodByAcademicYear))]
        [OpenApiOperation(tags: _tag, Summary = "Get Period By Academic Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPeriodByAcademicYearRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPeriodByAcademicYearRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetPeriodByAcademicYearResult>))]
        public Task<IActionResult> GetPeriodByAcademicYear(
               [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-period-by-academic-year")] HttpRequest req,
               CancellationToken cancellationToken)
        {
            return _getPeriodByAcademicYearHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PeriodEndpoint.GetCurrentPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Current Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCurrentPeriodRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCurrentPeriodResult))]
        public Task<IActionResult> GetCurrentPeriod(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/current-period")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getCurrentPeriodHandler.Execute(req, cancellationToken);
        }
    }
}
