using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.ExemplaryCharacter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.ExemplaryCharacter
{
    public class ExemplaryCharacterEndPoint
    {
        private const string _route = "exemplary-character";
        private const string _tag = "Exemplary Character";

        private readonly GetListExemplaryCharacterSummaryHandler _getListExemplaryCharacterSummaryHandler;
        private readonly GetDetailExemplaryCategorySettingsHandler _getDetailExemplaryCharacterSettingsHandler;
        private readonly DeleteExemplaryCharacterHandler _deleteExemplaryCharacterHandler;
        private readonly GetListExemplaryCategorySettingsHandler _getListExemplaryCategorySettingsHandler;
        private readonly CreateExemplaryCategorySettingsHandler _createExemplaryCategorySettingsHandler;
        private readonly UpdateExemplaryCategorySettingsHandler _updateExemplaryCategorySettingsHandler;
        private readonly DeleteExemplaryCategorySettingsHandler _deleteExemplaryCategorySettingsHandler;
        private readonly ExemplaryCharacterHandler _exemplaryCharacterHandler;
        private readonly GetExemplaryCharacterViewHandler _getExemplaryCharacterViewHandler;
        private readonly UpdateExemplaryLikeHandler _updateExemplaryLikeHandler;
        private readonly GetExemplaryCategoryUsedHandler _getExemplaryCategoryUsedHandler;
        private readonly GetExemplaryCharacterViewNewHandler _getExemplaryCharacterViewNewHandler;
        private readonly GetListStudentByAySmtLvlGrdHrmForExemplaryHandler _getListStudentByAySmtLvlGrdHrmForExemplaryHandler;
        private readonly GetListExemplaryValueSettingsHandler _getListExemplaryValueSettingsHandler;
        private readonly CreateExemplaryValueSettingsHandler _createExemplaryValueSettingsHandler;
        private readonly UpdateExemplaryValueSettingsHandler _updateExemplaryValueSettingsHandler;
        private readonly DeleteExemplaryValueSettingsHandler _deleteExemplaryValueSettingsHandler;

        public ExemplaryCharacterEndPoint(
            GetListExemplaryCharacterSummaryHandler getListExemplaryCharacterSummaryHandler,
            GetDetailExemplaryCategorySettingsHandler getDetailExemplaryCharacterSettingsHandler,
            DeleteExemplaryCharacterHandler deleteExemplaryCharacterHandler,
            GetListExemplaryCategorySettingsHandler getListExemplaryCategorySettingsHandler,
            CreateExemplaryCategorySettingsHandler createExemplaryCategorySettingsHandler,
            UpdateExemplaryCategorySettingsHandler updateExemplaryCategorySettingsHandler,
            DeleteExemplaryCategorySettingsHandler deleteExemplaryCategorySettingsHandler,
            ExemplaryCharacterHandler exemplaryCharacterHandler,
            GetExemplaryCharacterViewHandler getExemplaryCharacterViewHandler,
            UpdateExemplaryLikeHandler updateExemplaryLikeHandler,
            GetExemplaryCategoryUsedHandler getExemplaryCategoryUsedHandler,
            GetExemplaryCharacterViewNewHandler getExemplaryCharacterViewNewHandler,
            GetListStudentByAySmtLvlGrdHrmForExemplaryHandler getListStudentByAySmtLvlGrdHrmForExemplaryHandler,
            GetListExemplaryValueSettingsHandler getListExemplaryValueSettingsHandler,
            CreateExemplaryValueSettingsHandler createExemplaryValueSettingsHandler,
            UpdateExemplaryValueSettingsHandler updateExemplaryValueSettingsHandler,
            DeleteExemplaryValueSettingsHandler deleteExemplaryValueSettingsHandler
            )
        {
            _getListExemplaryCharacterSummaryHandler = getListExemplaryCharacterSummaryHandler;
            _getDetailExemplaryCharacterSettingsHandler = getDetailExemplaryCharacterSettingsHandler;
            _deleteExemplaryCharacterHandler = deleteExemplaryCharacterHandler;
            _getListExemplaryCategorySettingsHandler = getListExemplaryCategorySettingsHandler;
            _createExemplaryCategorySettingsHandler = createExemplaryCategorySettingsHandler;
            _updateExemplaryCategorySettingsHandler = updateExemplaryCategorySettingsHandler;
            _deleteExemplaryCategorySettingsHandler = deleteExemplaryCategorySettingsHandler;
            _exemplaryCharacterHandler = exemplaryCharacterHandler;
            _getExemplaryCharacterViewHandler = getExemplaryCharacterViewHandler;
            _updateExemplaryLikeHandler = updateExemplaryLikeHandler;
            _getExemplaryCategoryUsedHandler = getExemplaryCategoryUsedHandler;
            _getExemplaryCharacterViewNewHandler = getExemplaryCharacterViewNewHandler;
            _getListStudentByAySmtLvlGrdHrmForExemplaryHandler = getListStudentByAySmtLvlGrdHrmForExemplaryHandler;
            _getListExemplaryValueSettingsHandler = getListExemplaryValueSettingsHandler;
            _createExemplaryValueSettingsHandler = createExemplaryValueSettingsHandler;
            _updateExemplaryValueSettingsHandler = updateExemplaryValueSettingsHandler;
            _deleteExemplaryValueSettingsHandler = deleteExemplaryValueSettingsHandler;
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.GetListExemplaryCharacterSummary))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Exemplary Character Summary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetListExemplaryCharacterSummaryRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListExemplaryCharacterSummaryResult))]
        public Task<IActionResult> GetListExemplaryCharacterSummary(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-list-exemplary-character-summary")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListExemplaryCharacterSummaryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.GetListExemplaryCategorySettings))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Exemplary Category Settings")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListExemplaryCategorySettingsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExemplaryCategorySettingsRequest.Return), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExemplaryCategorySettingsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExemplaryCategorySettingsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExemplaryCategorySettingsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExemplaryCategorySettingsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExemplaryCategorySettingsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListExemplaryCategorySettingsRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListExemplaryCategorySettingsResult>))]
        public Task<IActionResult> GetListExemplaryCategorySettings(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-exemplary-category-settings")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListExemplaryCategorySettingsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.GetDetailExemplaryCategorySettings))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Exemplary Category Settings")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailExemplaryCategorySettingsRequest.IdExemplaryCharacter), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailExemplaryCategorySettingsResult))]
        public Task<IActionResult> GetDetailExemplaryCategorySettings(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-detail-exemplary-category-settings")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getDetailExemplaryCharacterSettingsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.DeleteExemplaryCharacter))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Exemplary Character")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteExemplaryCharacterRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteExemplaryCharacter(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-exemplary-character")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteExemplaryCharacterHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.CreateExemplaryCategorySettings))]
        [OpenApiOperation(tags: _tag, Summary = "Create Exemplary Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateExemplaryCategorySettingsRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CreateExemplaryCategorySettings(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/create-exemplary-category")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _createExemplaryCategorySettingsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.UpdateExemplaryCategorySettings))]
        [OpenApiOperation(tags: _tag, Summary = "Update Exemplary Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateExemplaryCategorySettingsRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateExemplaryCategorySettings(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-exemplary-category")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateExemplaryCategorySettingsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.DeleteExemplaryCategorySettings))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Exemplary Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteExemplaryCategorySettingsRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteExemplaryCategorySettings(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-exemplary-category")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteExemplaryCategorySettingsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.GetExemplaryCharacterById))]
        [OpenApiOperation(tags: _tag, Summary = "Get Exemplary Character By Id")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailExemplaryCharacterResult))]
        public Task<IActionResult> GetExemplaryCharacterById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/exemplary/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            return _exemplaryCharacterHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.SaveExemplaryCharacter))]
        [OpenApiOperation(tags: _tag, Summary = "Save and Update Exemplary Character")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(List<SaveExemplaryCharacterRequest>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveExemplaryCharacter(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/exemplary")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _exemplaryCharacterHandler.Execute(req, cancellationToken);
        }

        //[FunctionName(nameof(ExemplaryCharacterEndPoint.DeleteExemplaryCharacter))]
        //[OpenApiOperation(tags: _tag, Summary = "Delete Exemplary Character")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        //[OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        //public Task<IActionResult> DeleteExemplaryCharacter(
        //[HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/exemplary")] HttpRequest req,
        //CancellationToken cancellationToken)
        //{
        //    return _exemplaryCharacterHandler.Execute(req, cancellationToken);
        //}

        [FunctionName(nameof(ExemplaryCharacterEndPoint.GetExemplaryCharacterView))]
        [OpenApiOperation(tags: _tag, Summary = "Get Exemplary Character View")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExemplaryCharacterViewRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetExemplaryCharacterViewRequest.Return), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetExemplaryCharacterViewRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetExemplaryCharacterViewRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetExemplaryCharacterViewRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetExemplaryCharacterViewRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExemplaryCharacterViewRequest.IdUserRequested), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExemplaryCharacterViewRequest.Type), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExemplaryCharacterViewRequest.IdValueList), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetExemplaryCharacterViewRequest.IsParent), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListExemplaryValueSettingsResult>))]
        public Task<IActionResult> GetExemplaryCharacterView(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-view-exemplary-character")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getExemplaryCharacterViewNewHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.UpdateExemplaryLike))]
        [OpenApiOperation(tags: _tag, Summary = "Update Exemplary Like")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateExemplaryLikeRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UpdateExemplaryLikeResult))]
        public Task<IActionResult> UpdateExemplaryLike(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-exemplary-like")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateExemplaryLikeHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ExemplaryCharacterEndPoint.GetExemplaryCategoryUsed))]
        [OpenApiOperation(tags: _tag, Summary = "Get Exemplary Category Used")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExemplaryCategoryUsedRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListExemplaryCategorySettingsResult>))]
        public Task<IActionResult> GetExemplaryCategoryUsed(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-exemplary-category-used")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getExemplaryCategoryUsedHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.GetListStudentByAySmtLvlGrdHrmForExemplary))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Student By Ay Smt Lvl Grd Hrm For Exemplary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByAySmtLvlGrdHrmForExemplaryRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListStudentByAySmtLvlGrdHrmForExemplaryResult>))]
        public Task<IActionResult> GetListStudentByAySmtLvlGrdHrmForExemplary(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-student-for-exemplary")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getListStudentByAySmtLvlGrdHrmForExemplaryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.GetListExemplaryValueSettings))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Exemplary Value Settings")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListExemplaryValueSettingsRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExemplaryValueSettingsRequest.Return), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExemplaryValueSettingsRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExemplaryValueSettingsRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExemplaryValueSettingsRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExemplaryValueSettingsRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExemplaryValueSettingsRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListExemplaryValueSettingsRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetListExemplaryValueSettingsResult>))]
        public Task<IActionResult> GetListExemplaryValueSettings(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-exemplary-value-settings")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListExemplaryValueSettingsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.CreateExemplaryValueSettings))]
        [OpenApiOperation(tags: _tag, Summary = "Create Exemplary Value")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateExemplaryValueSettingsRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CreateExemplaryValueSettings(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/create-exemplary-value")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _createExemplaryValueSettingsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.UpdateExemplaryValueSettings))]
        [OpenApiOperation(tags: _tag, Summary = "Update Exemplary Value")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateExemplaryValueSettingsRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateExemplaryValueSettings(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-exemplary-value")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateExemplaryValueSettingsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExemplaryCharacterEndPoint.DeleteExemplaryValueSettings))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Exemplary Value")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteExemplaryValueSettingsRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteExemplaryValueSettings(
          [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-exemplary-value")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteExemplaryValueSettingsHandler.Execute(req, cancellationToken);
        }
    }
}
