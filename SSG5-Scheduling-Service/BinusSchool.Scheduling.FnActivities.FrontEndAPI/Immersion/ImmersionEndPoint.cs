using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.ImmersionPeriod;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using BinusSchool.Scheduling.FnActivities.Immersion.ImmersionPeriod;
using BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnActivities.Immersion
{
    public class ImmersionEndPoint
    {
        private const string _route = "immersion";
        private const string _tag = "Immersion";

        private readonly GetImmersionPeriodHandler _getImmersionPeriodHandler;
        private readonly GetImmersionPeriodDetailHandler _getImmersionPeriodDetailHandler;
        private readonly UpdateImmersionPeriodHandler _updateImmersionPeriodHandler;
        private readonly DeleteImmersionPeriodHandler _deleteImmersionPeriodHandler;

        private readonly GetMasterImmersionHandler _getMasterImmersionHandler;
        private readonly GetMasterImmersionDetailHandler _getMasterImmersionDetailHandler;
        private readonly ExportExcelMasterImmersionHandler _exportExcelMasterImmersionHandler;
        private readonly ImmersionDocumentHandler _immersionDocumentHandler;
        private readonly GetCurrencyHandler _getCurrencyHandler;
        private readonly GetImmersionPaymentMethodHandler _getImmersionPaymentMethodHandler;
        private readonly AddMasterImmersionHandler _addMasterImmersionHandler;
        private readonly UpdateMasterImmersionHandler _updateMasterImmersionHandler;
        private readonly DeleteMasterImmersionHandler _deleteMasterImmersionHandler;

        public ImmersionEndPoint(
            GetImmersionPeriodHandler getImmersionPeriodHandler,
            GetImmersionPeriodDetailHandler getImmersionPeriodDetailHandler,
            UpdateImmersionPeriodHandler updateImmersionPeriodHandler,
            DeleteImmersionPeriodHandler deleteImmersionPeriodHandler,

            GetMasterImmersionHandler getMasterImmersionHandler,
            GetMasterImmersionDetailHandler getMasterImmersionDetailHandler,
            ExportExcelMasterImmersionHandler exportExcelMasterImmersionHandler,
            ImmersionDocumentHandler immersionDocumentHandler,
            GetCurrencyHandler getCurrencyHandler,
            GetImmersionPaymentMethodHandler getImmersionPaymentMethodHandler,
            AddMasterImmersionHandler addMasterImmersionHandler,
            UpdateMasterImmersionHandler updateMasterImmersionHandler,
            DeleteMasterImmersionHandler deleteMasterImmersionHandler)
        {
            _getImmersionPeriodHandler = getImmersionPeriodHandler;
            _getImmersionPeriodDetailHandler = getImmersionPeriodDetailHandler;
            _exportExcelMasterImmersionHandler = exportExcelMasterImmersionHandler;
            _updateImmersionPeriodHandler = updateImmersionPeriodHandler;
            _deleteImmersionPeriodHandler = deleteImmersionPeriodHandler;

            _getMasterImmersionHandler = getMasterImmersionHandler;
            _getMasterImmersionDetailHandler = getMasterImmersionDetailHandler;
            _immersionDocumentHandler = immersionDocumentHandler;
            _getCurrencyHandler = getCurrencyHandler;
            _getImmersionPaymentMethodHandler = getImmersionPaymentMethodHandler;
            _addMasterImmersionHandler = addMasterImmersionHandler;
            _updateMasterImmersionHandler = updateMasterImmersionHandler;
            _deleteMasterImmersionHandler = deleteMasterImmersionHandler;
        }

        #region Immersion Period
        [FunctionName(nameof(ImmersionEndPoint.GetImmersionPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Immersion Period List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetImmersionPeriodRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetImmersionPeriodRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetImmersionPeriodRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetImmersionPeriodRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetImmersionPeriodRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetImmersionPeriodRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetImmersionPeriodRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetImmersionPeriodRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetImmersionPeriodRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetImmersionPeriodResult>))]
        public Task<IActionResult> GetImmersionPeriod([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/immersion-period")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getImmersionPeriodHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.GetImmersionPeriodDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Immersion Period Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetImmersionPeriodDetailRequest.IdImmersionPeriod), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetImmersionPeriodDetailResult))]
        public Task<IActionResult> GetImmersionPeriodDetail([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/immersion-period-detail")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getImmersionPeriodDetailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.UpdateImmersionPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Add / Update Immersion Period", Description = "<b>Remove <u>IdImmersionPeriod</u> from the parameter if you want to add new immersion period</b>")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateImmersionPeriodRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateImmersionPeriod([HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/immersion-period")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateImmersionPeriodHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.DeleteImmersionPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Immersion Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteImmersionPeriodRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteImmersionPeriod([HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/immersion-period")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteImmersionPeriodHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Master Immersion
        [FunctionName(nameof(ImmersionEndPoint.GetMasterImmersion))]
        [OpenApiOperation(tags: _tag, Summary = "Get Master Immersion List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMasterImmersionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterImmersionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterImmersionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterImmersionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterImmersionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterImmersionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterImmersionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetMasterImmersionRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMasterImmersionRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetMasterImmersionRequest.IdImmersionPeriod), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetMasterImmersionResult>))]
        public Task<IActionResult> GetMasterImmersion([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/master-immersion")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getMasterImmersionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.GetMasterImmersionDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Master Immersion Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMasterImmersionDetailRequest.IdImmersion), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterImmersionDetailResult))]
        public Task<IActionResult> GetMasterImmersionDetail([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/master-immersion-detail")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getMasterImmersionDetailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.ExportExcelMasterImmersion))]
        [OpenApiOperation(tags: _tag, Summary = "Export Master Immersion to Excel")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelMasterImmersionRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelMasterImmersion([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/master-immersion-excel")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _exportExcelMasterImmersionHandler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ImmersionEndPoint.AddMasterImmersion))]
        [OpenApiOperation(tags: _tag, Summary = "Add Master Immersion")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMasterImmersionRequest), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddMasterImmersionResult))]
        public Task<IActionResult> AddMasterImmersion([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/master-immersion")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _addMasterImmersionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.UpdateMasterImmersion))]
        [OpenApiOperation(tags: _tag, Summary = "Update Master Immersion")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMasterImmersionRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateMasterImmersion([HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/master-immersion")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateMasterImmersionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.DeleteMasterImmersion))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Master Immersion")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteMasterImmersionRequest), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteMasterImmersion(
         [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/master-immersion")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _deleteMasterImmersionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.GetImmersionDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Get Immersion Document List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(ImmersionDocumentRequest_Get))]
        [OpenApiParameter(nameof(ImmersionDocumentRequest_Get.IdImmersions), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<ImmersionDocumentResult_Get>))]       
        public Task<IActionResult> GetImmersionDocument([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/immersion-doc")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _immersionDocumentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.GetImmersionDocumentDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Immersion Document Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ImmersionDocumentResult_GetDetail))]
        public Task<IActionResult> GetImmersionDocumentDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/immersion-doc/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            return _immersionDocumentHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        //[FunctionName(nameof(ImmersionEndPoint.AddImmersionDocument))]
        //[OpenApiOperation(tags: _tag, Summary = "Add Immersion Document")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        ////[OpenApiRequestBzody("application/json", typeof(UpdateSupportingDocumentRequest))]
        ////[OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        ////[OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]       
        //[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ImmersionDocumentRequest_Post))]
        //public Task<IActionResult> AddImmersionDocument(
        //[HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/immersion-doc")] HttpRequest req,
        //CancellationToken cancellationToken)
        //{
        //    return _immersionDocumentHandler.Execute(req, cancellationToken, true);
        //}

        [FunctionName(nameof(ImmersionEndPoint.UpdateImmersionDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Update Immersion Document")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(UpdateSupportingDocumentRequest))]
        //[OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ImmersionDocumentRequest_Put))]
        public Task<IActionResult> UpdateImmersionDocument(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/immersion-doc")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _immersionDocumentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.DeleteImmersionDocument))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Immersion Document")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteImmersionDocument(
         [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/immersion-doc")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _immersionDocumentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.GetCurrency))]
        [OpenApiOperation(tags: _tag, Summary = "Get Currency")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCurrencyRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCurrencyRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCurrencyRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCurrencyRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetCurrencyRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCurrencyRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetCurrencyRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetCurrencyRequest.IdCurrency), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCurrencyResult))]
        public Task<IActionResult> GetCurrency([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-currency")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getCurrencyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ImmersionEndPoint.GetImmersionPaymentMethod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Immersion Payment Method")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetImmersionPaymentMethodRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetImmersionPaymentMethodRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetImmersionPaymentMethodRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetImmersionPaymentMethodRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetImmersionPaymentMethodRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetImmersionPaymentMethodRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetImmersionPaymentMethodRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetImmersionPaymentMethodRequest.IdImmersionPaymentMethod), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetImmersionPaymentMethodResult))]
        public Task<IActionResult> GetImmersionPaymentMethod([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-immersion-payment-method")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getImmersionPaymentMethodHandler.Execute(req, cancellationToken);
        }
        #endregion
    }
}
