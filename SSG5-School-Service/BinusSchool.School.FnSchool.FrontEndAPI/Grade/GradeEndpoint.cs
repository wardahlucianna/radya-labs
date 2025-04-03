using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.GradePathwayDetails;
using BinusSchool.Data.Model.School.FnSchool.GradePathways;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using BinusSchool.Common.Model;

namespace BinusSchool.School.FnSchool.Grade
{
    public class GradeEndpoint
    {
        private const string _route = "school/grade";
        private const string _tag = "School Grade";

        [FunctionName(nameof(GradeEndpoint.GetListGradePathway))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade pathway List by grade pathways ids for upload asc time table ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradepathwayForXMLRequest.IdGradePathway), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GradePathwayForAscTimeTableResult))]
        public Task<IActionResult> GetListGradePathway(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"-by-grade-pathway-ids")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradePathwayForAscTimetableHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GetListGradePathwayBySessionSet))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade pathway List by session set for upload asc time table ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GradePathwayBySessionSetIdForXmlRequest.IdSessionset), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GradePathwayForAscTimeTableResult))]
        public Task<IActionResult> GetListGradePathwayBySessionSet(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-sessionset")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradePathwayBySessionSetHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GetListGradeByListId))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade List by ids for upload asc time table ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeCodeByIDForAscTimetableRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetListGradeByListId(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-ids")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGradeCodeByIDForAscTimetableHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GetGrades))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetGradeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGradeRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeRequest.IsRemoveLastGrade), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeResult))]
        public Task<IActionResult> GetGrades(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GetGradeDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeDetailResult))]
        public Task<IActionResult> GetGradeDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradeHandler>();
            return handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(GradeEndpoint.AddGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Add Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddGradeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddGrade(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradeHandler>();
            return handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(GradeEndpoint.UpdateGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Update Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateGradeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateGrade(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.DeleteGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteGrade(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GetGradAcadYears))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade List Acadyears")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeAcadyearRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeAcadyearRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeAcadyearRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeAcadyearRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeAcadyearRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeAcadyearRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeAcadyearRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradeAcadyearRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGradeAcadyearRequest.ExcludeHavePeriod), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradeAcadyearRequest.ExcludeHaveSubject), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradeAcadyearRequest.ExcludeHavePathway), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeAcadyearResult))]
        public Task<IActionResult> GetGradAcadYears(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/grade-acadyear")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradeAcademicYearsHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GradeCodeAcademicYears))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade Code List Acadyears")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GradeCodeAcademicYearsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GradeCodeAcademicYearsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GradeCodeAcademicYearsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GradeCodeAcademicYearsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GradeCodeAcademicYearsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GradeCodeAcademicYearsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GradeCodeAcademicYearsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GradeCodeAcademicYearsRequest.IdSchool), In = ParameterLocation.Query , Required = true)]
        [OpenApiParameter(nameof(GradeCodeAcademicYearsRequest.CodeAcademicYearStart), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GradeCodeAcademicYearsRequest.CodeAcademicYearEnd), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GradeCodeAcademicYearsResult))]
        public Task<IActionResult> GradeCodeAcademicYears(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/grade-code-acadyear")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradeCodeAcademicYearsHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GetGradePathwayDetails))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade Details")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradePathwayDetailRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwayDetailRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwayDetailRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradePathwayDetailRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradePathwayDetailRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwayDetailRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwayDetailRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradePathwayDetailRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradePathwayResult))]
        public Task<IActionResult> GetGradePathwayDetails(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/grade-pathway-detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradePathwayDetailHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GetGradWithPathway))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade pathway List ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeWithPathwayRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeWithPathwayRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeWithPathwayRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeWithPathwayRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeWithPathwayRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeWithPathwayRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeWithPathwayRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradeWithPathwayRequest.IdAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGradeWithPathwayRequest.IdSessionSet), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeWithPathwayResult))]
        public Task<IActionResult> GetGradWithPathway(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/grade-with-pathway")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradeWithPathwayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GetGradPathway))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade pathway List ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradePathwayRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwayRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwayRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradePathwayRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradePathwayRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwayRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwayRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradePathwayRequest.IdSchoolAcadyear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeWithPathwayResult))]
        public Task<IActionResult> GetGradPathway(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/grade-pathway")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradePathwayHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GradePathwaySummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade pathway List ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradePathwaySummaryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwaySummaryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwaySummaryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradePathwaySummaryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradePathwaySummaryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwaySummaryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradePathwaySummaryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradePathwaySummaryRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeWithPathwayResult))]
        public Task<IActionResult> GradePathwaySummary(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/grade-with-pathway-summary")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradePathwaySummaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GetGradeCodeList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeCodeRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetGradeCodeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeCodeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeCodeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeCodeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetGradeCodeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeCodeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeCodeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradeCodeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGradeCodeRequest.CodeAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeCodeRequest.CodeLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeCodeResult))]
        public Task<IActionResult> GetGradeCodeList(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-code")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GradeCodeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(GradeEndpoint.GetGradeMultipleLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade By Multiple Level")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGradeMultipleLevelRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGradeMultipleLevelRequest.IdAcadyear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetGradeMultipleLevelRequest.IdLevel), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeMultipleLevelResult))]
        public Task<IActionResult> GetGradeMultipleLevel(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "school/grade-by-multiple-level")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGradeMultipleLevelHandlerHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
