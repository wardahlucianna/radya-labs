using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterRule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterRule
{
    public class MasterExtracurricularRuleEndPoint
    {
        private const string _route = "extracurricular/master-rule";
        private const string _tag = "Extracurricular Master Rules";

        private readonly MasterExtracurricularRuleHandler _masterextracurricularrulehandler;
        private readonly SupportingDucumentHandler _supportingducumenthandler;
        private readonly CopyExtracurricularRuleFromLastAYHandler _copyExtracurricularRuleFromLastAYHandler;
        

        public MasterExtracurricularRuleEndPoint(
           MasterExtracurricularRuleHandler masterextracurricularrulehandler,
           SupportingDucumentHandler supportingducumenthandler,
           CopyExtracurricularRuleFromLastAYHandler copyExtracurricularRuleFromLastAYHandler)
        {
            _masterextracurricularrulehandler = masterextracurricularrulehandler;
            _supportingducumenthandler = supportingducumenthandler;
            _copyExtracurricularRuleFromLastAYHandler = copyExtracurricularRuleFromLastAYHandler;
        }

        [FunctionName(nameof(MasterExtracurricularRuleEndPoint.GetMasterExtracurricularRule))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Master Rule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRuleRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRuleRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRuleRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterExtracurricularRuleRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterExtracurricularRuleRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRuleRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRuleRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetMasterExtracurricularRuleRequest.IdSchool), In = ParameterLocation.Query,Required = true)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRuleRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRuleRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterExtracurricularRuleRequest.Status), In = ParameterLocation.Query, Description = "type boolean")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetMasterExtracurricularRuleResult>))]
        public Task<IActionResult> GetMasterExtracurricularRule([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _masterextracurricularrulehandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularRuleEndPoint.GetMasterExtracurricularRuleDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Master Rule Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterExtracurricularRuleResult))]
        public Task<IActionResult> GetMasterExtracurricularRuleDetail(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
       string id,
       CancellationToken cancellationToken)
        {
            return _masterextracurricularrulehandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(MasterExtracurricularRuleEndPoint.AddMasterExtracurricularRule))]
        [OpenApiOperation(tags: _tag, Summary = "Add Master Extracurricular Rule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMasterExtracurricularRuleRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMasterExtracurricularRule(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _masterextracurricularrulehandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularRuleEndPoint.UpdateMasterExtracurricularRule))]
        [OpenApiOperation(tags: _tag, Summary = "Update Master Extracurricular Rule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMasterExtracurricularRuleRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateMasterExtracurricularRule(
       [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _masterextracurricularrulehandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(MasterExtracurricularRuleEndPoint.DeleteMasterExtracurricularRule))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Master Extracurricular Rule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteMasterExtracurricularRule(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _masterextracurricularrulehandler.Execute(req, cancellationToken);
        }



        [FunctionName(nameof(MasterExtracurricularRuleEndPoint.GetSupportingDucument))]
        [OpenApiOperation(tags: _tag, Summary = "Get Supporting Document Master Rule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSupportingDucumentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSupportingDucumentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSupportingDucumentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSupportingDucumentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetSupportingDucumentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSupportingDucumentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSupportingDucumentRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetSupportingDucumentRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSupportingDucumentRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetSupportingDucumentRequest.Status), In = ParameterLocation.Query, Description = "type boolean")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSupportingDucumentResult>))]       
        public Task<IActionResult> GetSupportingDucument([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-support-doc")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _supportingducumenthandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(MasterExtracurricularRuleEndPoint.GetSupportingDucumentDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Supporting Document Master Rule Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSupportingDucumentDetailResult))]
        public Task<IActionResult> GetSupportingDucumentDetail(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-support-doc/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            return _supportingducumenthandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }


        [FunctionName(nameof(MasterExtracurricularRuleEndPoint.AddSupportingDucument))]
        [OpenApiOperation(tags: _tag, Summary = "Add Supporting Document Master Rule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBzody("application/json", typeof(UpdateSupportingDocumentRequest))]
        //[OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        //[OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]       
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddSupportingDocumentRequest))]   
        public Task<IActionResult> AddSupportingDucument(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-support-doc")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _supportingducumenthandler.Execute(req, cancellationToken,true);
        }
               

        [FunctionName(nameof(MasterExtracurricularRuleEndPoint.UpdateSupportingDucument))]
        [OpenApiOperation(tags: _tag, Summary = "Update Supporting Document Master Rule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(UpdateSupportingDocumentRequest))]
        //[OpenApiRequestBody("multipart/form-data", typeof(IFormFile), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UpdateSupportingDocumentRequest))]
        public Task<IActionResult> UpdateSupportingDucument(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-support-doc")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _supportingducumenthandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularRuleEndPoint.DeleteSupportingDucument))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Supporting Document Master Rule")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteSupportingDucument(
         [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "-support-doc")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _supportingducumenthandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MasterExtracurricularRuleEndPoint.CopyExtracurricularRuleFromLastAY))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Extracurricular Rule From Last AY")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopyExtracurricularRuleFromLastAYRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopyExtracurricularRuleFromLastAY(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/copy-extracurricular-rule-from-last-ay")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _copyExtracurricularRuleFromLastAYHandler.Execute(req, cancellationToken);
        }
    }
}
