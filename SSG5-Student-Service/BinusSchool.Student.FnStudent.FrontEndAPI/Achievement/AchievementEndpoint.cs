using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using BinusSchool.Student.FnStudent.MeritDemerit;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Student.FnStudent.Portfolio.Coursework;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.Achievement
{
    public class AchievementEndpoint
    {
        private const string _route = "student/achievement";
        private const string _tag = "Achievement";

        private readonly AchievementHandler _achievement;
        public AchievementEndpoint(AchievementHandler Achievement)
        {
            _achievement = Achievement;
        }

        #region My Achievement
        [FunctionName(nameof(AchievementEndpoint.GetAchievement))]
        [OpenApiOperation(tags: _tag, Summary = "Get Achievement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAchievementRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAchievementRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAchievementRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAchievementRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAchievementRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAchievementRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAchievementRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetAchievementRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAchievementRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAchievementRequest.Status), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAchievementRequest.Type), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAchievementRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAchievementRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAchievementResult[]))]
        public Task<IActionResult> GetAchievement(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _achievement.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AchievementEndpoint.AddAchievement))]
        [OpenApiOperation(tags: _tag, Summary = "Create Achievement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddAchievementRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddAchievement(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            [Queue("notification-student-achievement")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            return _achievement.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(AchievementEndpoint.DetailAchievement))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Achievement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailAchievementResult))]
        public Task<IActionResult> DetailAchievement(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
        CancellationToken cancellationToken)
        {
            return _achievement.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(AchievementEndpoint.UpdateAchievement))]
        [OpenApiOperation(tags: _tag, Summary = "Update Achievement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddAchievementRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateAchievement(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _achievement.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AchievementEndpoint.GetUserByPosition))]
        [OpenApiOperation(tags: _tag, Summary = "Get User By Position")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserByPositionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserByPositionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserByPositionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUserByPositionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUserByPositionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserByPositionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserByPositionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUserByPositionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserByPositionRequest.CodePosition), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAchievementResult[]))]
        public Task<IActionResult> GetUserByPosition(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+ "-user-by-position")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUserByPositionHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AchievementEndpoint.GetFocusArea))]
        [OpenApiOperation(tags: _tag, Summary = "Get Focus Area")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm[]))]
        public Task<IActionResult> GetFocusArea(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-focus-area")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetFocusAreaHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AchievementEndpoint.DownloadAchievement))]
        [OpenApiOperation(tags: _tag, Summary = "Download Achievement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetAchievementRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DownloadAchievement(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-download")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadAchievementHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
        #endregion


        #region Approval Achievement
        [FunctionName(nameof(AchievementEndpoint.ApprovalAndDeleteAchievement))]
        [OpenApiOperation(tags: _tag, Summary = "Approval and Delete Achievement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ApprovalAndDeleteAchievementRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> ApprovalAndDeleteAchievement(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-delete-approval")] HttpRequest req,
        [Queue("notification-student-achievement")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ApprovalAndDeleteAchievementHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
        #endregion
    }
}
