using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.LearningGolas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.Portfolio.LearningGoals
{
    public class LearningGoalsEndPoint
    {
        private const string _route = "student/portfolio";
        private const string _tag = "Learning Goals";

        private readonly LearningGoalsHandler _learningGoalsHandler;
        private readonly LearningGoalsAchievedHandler _learningGoalsAchievedHandler;

        public LearningGoalsEndPoint(LearningGoalsHandler LearningGoalsHandler, LearningGoalsAchievedHandler LearningGoalsAchievedHandler)
        {
            _learningGoalsHandler = LearningGoalsHandler;
            _learningGoalsAchievedHandler = LearningGoalsAchievedHandler;
        }


        [FunctionName(nameof(LearningGoalsEndPoint.GetLearningGoals))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lerning Goals On Going")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLearningGoalsOnGoingResult[]))]
        public Task<IActionResult> GetLearningGoals(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/learning-goals")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _learningGoalsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LearningGoalsEndPoint.GetDetailLearningGoals))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Lerning Goals Achieved")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailLearningGoalsResult))]
        public Task<IActionResult> GetDetailLearningGoals(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/learning-goals/{id}")] HttpRequest req,
           string id,
        CancellationToken cancellationToken)
        {
            return _learningGoalsHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(LearningGoalsEndPoint.AddLearningGoals))]
        [OpenApiOperation(tags: _tag, Summary = "Add Lerning Goals Achieved")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddLearningGoalsRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddLearningGoals(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/learning-goals")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _learningGoalsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LearningGoalsEndPoint.UpdateLearningGoals))]
        [OpenApiOperation(tags: _tag, Summary = "Update Lerning Goals Achieved")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateLearningGoalsRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateLearningGoals(
       [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/learning-goals")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _learningGoalsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LearningGoalsEndPoint.DeleteLearningGoals))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Lerning Goals On going and Achieved")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteLearningGoals(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/learning-goals")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _learningGoalsHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(LearningGoalsEndPoint.GetLearningAchieved))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lerning Goals Achieved")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetLearningGoalsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLearningGoalsAchievedResult[]))]
        public Task<IActionResult> GetLearningAchieved(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/learning-goals-archieved")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _learningGoalsAchievedHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LearningGoalsEndPoint.UpdateLearningAchieved))]
        [OpenApiOperation(tags: _tag, Summary = "Update Status Lerning Goals OnGoing to Achieved")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateLearningAchievedRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateLearningAchieved(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/learning-goals-archieved")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _learningGoalsAchievedHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LearningGoalsEndPoint.GetLearningGoalsButton))]
        [OpenApiOperation(tags: _tag, Summary = "Get Lerning Goals On Going")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLearningGoalsButtonRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsButtonRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsButtonRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsButtonRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsButtonRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsButtonRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsButtonRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLearningGoalsButtonRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLearningGoalsButtonResult))]
        public Task<IActionResult> GetLearningGoalsButton(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/learning-goals-button")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetLearningGoalsButtonHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
