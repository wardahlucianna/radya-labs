using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.MeritDemerit
{
    public class MeritDemeritEndPoint
    {
        private const string _route = "school/merit-demerit";
        private const string _tag = "Merit and Demerit";

        private readonly MeritDemeritDisciplineHandler _disciplineHandler;
        private readonly MeritDemeritSanctionHandler _sanctionHandler;
        private readonly MeritDemeritLevelInfractionHandler _levelInfractionHandler;
        private readonly ScoreContinuationSettingHeandler _scoreContinuationHandler;
        public MeritDemeritEndPoint(MeritDemeritDisciplineHandler DisciplineHandler, MeritDemeritSanctionHandler SanctionHandler, MeritDemeritLevelInfractionHandler LevelInfractionHandler, ScoreContinuationSettingHeandler scoreContinuationHandler)
        {
            _disciplineHandler = DisciplineHandler;
            _sanctionHandler = SanctionHandler;
            _levelInfractionHandler = LevelInfractionHandler;
            _scoreContinuationHandler = scoreContinuationHandler;
        }


        #region Merit and Demerit Discipline Mapping
        [FunctionName(nameof(MeritDemeritEndPoint.AddMeritDemeritDisciplineMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Add Merit and Demerit Discipline Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMeritDemeritDisciplineMappingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddMeritDemeritDisciplineMapping(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/discipline-mapping")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _disciplineHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.DeleteMeritDemeritDisciplineMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Merit and Demerit Discipline Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteMeritDemeritDisciplineMapping(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/discipline-mapping")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _disciplineHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.UpdateMeritDemeritDisciplineMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Update Merit and Demerit Discipline Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMeritDemeritDisciplineMappingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateMeritDemeritDisciplineMapping(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/discipline-mapping")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _disciplineHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.GetMeritDemeritDisciplineMappingDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Merit and Demerit Discipline Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMeritDemeritDisciplineMappingDetailResult))]
        public Task<IActionResult> GetMeritDemeritDisciplineMappingDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/discipline-mapping/{id}")] HttpRequest req,
           string id,
        CancellationToken cancellationToken)
        {
            return _disciplineHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(MeritDemeritEndPoint.GetMeritDemeritDisciplineMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Get Merit and Demerit Discipline Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.IdAcademiYear), In = ParameterLocation.Query,Required = true)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.Category), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.IdLevelInfraction), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMeritDemeritDisciplineMappingResult[]))]
        public Task<IActionResult> GetMeritDemeritDisciplineMapping(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/discipline-mapping")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _disciplineHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.GetMeritDemeritDisciplineMappingLevel))]
        [OpenApiOperation(tags: _tag, Summary = "Get Merit and Demerit Level of Infraction")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMeritDemeritLevelInfractionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm[]))]
        public Task<IActionResult> GetMeritDemeritDisciplineMappingLevel(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/discipline-mapping-level")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMeritDemeritDisciplineMappingLevelHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.GetMeritDemeritDisciplineMappingCheckPoint))]
        [OpenApiOperation(tags: _tag, Summary = "Get Merit and Demerit Check Point")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMeritDemeritComponentSettingRequest.Idschool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMeritDemeritComponentSettingRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMeritDemeritDisciplineMappingCheckPointResult))]

        public Task<IActionResult> GetMeritDemeritDisciplineMappingCheckPoint(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/discipline-mapping-check-point")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMeritDemeritDisciplineMappingCheckPointHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.AddMeritDemeritDisciplineMappingCopy))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Merit and Demerit Discipline Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMeritDemeritDisciplineMappingCopyRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddMeritDemeritDisciplineMappingCopyResult[]))]

        public Task<IActionResult> AddMeritDemeritDisciplineMappingCopy(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/discipline-mapping-copy")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddMeritDemeritDisciplineMappingCopyHandler>();
            return handler.Execute(req, cancellationToken);
        }

        #endregion

        #region Merit and Demerit Sanction Mapping
        [FunctionName(nameof(MeritDemeritEndPoint.GetMeritDemeritSanctionMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Get Merit and Demerit Sanction Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMeritDemeritSanctionMappingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineMappingRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMeritDemeritSanctionMappingResult[]))]
        public Task<IActionResult> GetMeritDemeritSanctionMapping(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/Sanction-mapping")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _sanctionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.AddMeritDemeritSanctionMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Get Merit and Demerit Sanction Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMeritDemeritSanctionMappingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddMeritDemeritSanctionMapping(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/Sanction-mapping")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _sanctionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.UpdateMeritDemeritSanctionMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Update Merit and Demerit Sanction Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMeritDemeritSanctionMappingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateMeritDemeritSanctionMapping(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/Sanction-mapping")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _sanctionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.GetMeritDemeritSanctionMappingDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Merit and Demerit Sanction Mapping Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMeritDemeritSanctionMappingDetailResult))]
        public Task<IActionResult> GetMeritDemeritSanctionMappingDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/Sanction-mapping/{id}")] HttpRequest req,string id,
        CancellationToken cancellationToken)
        {
            return _sanctionHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(MeritDemeritEndPoint.DeteleMeritDemeritSanctionMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Merit and Demerit Sanction Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeteleMeritDemeritSanctionMapping(
        [HttpTrigger(AuthorizationLevel.Function, "Delete", Route = _route + "/Sanction-mapping")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _sanctionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.GetMeritDemeritSanctionMappingCopy))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Sunction Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMeritDemeritSanctionMappingCopyRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddMeritDemeritSanctionMappingCopyResult[]))]

        public Task<IActionResult> GetMeritDemeritSanctionMappingCopy(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/Sanction-mapping-copy")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddMeritDemeritSanctionMappingCopyHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Merit and Demerit Level Infraction
        [FunctionName(nameof(MeritDemeritEndPoint.AddMeritDemeritLevelInfraction))]
        [OpenApiOperation(tags: _tag, Summary = "Add and update Merit and Demerit Level Infraction")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMeritDemeritLevelInfractionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddMeritDemeritLevelInfraction(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/level-infraction")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _levelInfractionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.GetMeritDemeritLevelInfraction))]
        [OpenApiOperation(tags: _tag, Summary = "Get Merit and Demerit Level Infraction")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMeritDemeritLevelInfractionRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMeritDemeritLevelInfractionResult[]))]
        public Task<IActionResult> GetMeritDemeritLevelInfraction(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/level-infraction")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _levelInfractionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.DeleteMeritDemeritLevelInfraction))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Merit and Demerit Level Infraction")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteMeritDemeritLevelInfraction(
      [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/level-infraction")] HttpRequest req,
      CancellationToken cancellationToken)
        {
            return _levelInfractionHandler.Execute(req, cancellationToken);
        }

        #endregion

        #region Merit and Demerit Component Setting
        [FunctionName(nameof(MeritDemeritEndPoint.GetMeritDemeritComponentSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Merit and Demerit Component Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMeritDemeritComponentSettingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMeritDemeritComponentSettingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritComponentSettingRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritComponentSettingRequest.Idschool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMeritDemeritComponentSettingResult[]))]
        public Task<IActionResult> GetMeritDemeritComponentSetting(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/component-setting")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMeritDemeritComponentSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.UpdateMeritDemeritComponentSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Update Merit and Demerit Component Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMeritDemeritComponentSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateMeritDemeritComponentSetting(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/component-setting")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateMeritDemeritComponentSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Merit and Demerit Discipline Aproval
        [FunctionName(nameof(MeritDemeritEndPoint.GetMeritDemeritDisciplineAproval))]
        [OpenApiOperation(tags: _tag, Summary = "Get Merit and Demerit Discipline Aproval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineAprovalRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineAprovalRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMeritDemeritDisciplineAprovalRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMeritDemeritDisciplineAprovalResult[]))]
        public Task<IActionResult> GetMeritDemeritDisciplineAproval(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/discipline-aproval")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetMeritDemeritDisciplineAprovalHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.UpdateMeritDemeritDisciplineAproval))]
        [OpenApiOperation(tags: _tag, Summary = "Update Merit and Demerit Discipline Aproval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMeritDemeritDisciplineAprovalRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateMeritDemeritDisciplineAproval(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/discipline-aproval")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateMeritDemeritDisciplineAprovalHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Score Continuation
        [FunctionName(nameof(MeritDemeritEndPoint.UpdateScoreContinuationSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Add and Update Score Continuation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateScoreContinuationSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateScoreContinuationSetting(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/score-continuation")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _scoreContinuationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritEndPoint.GetScoreContinuationSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Score Continuation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetScoreContinuationSettingRequest.IdAcademiYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetScoreContinuationSettingRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetScoreContinuationSettingRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetScoreContinuationSettingResult[]))]
        public Task<IActionResult> GetScoreContinuationSetting(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/score-continuation")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _scoreContinuationHandler.Execute(req, cancellationToken);
        }

        

        [FunctionName(nameof(MeritDemeritEndPoint.ResetScoreContinuationSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Reset Score Continuation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetScoreContinuationSettingRequest.IdAcademiYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> ResetScoreContinuationSetting(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/score-continuation")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _scoreContinuationHandler.Execute(req, cancellationToken);
        }
        #endregion
    }
}
