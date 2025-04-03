using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Document.FnDocument.ConsentForm;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Document.FnDocument.ConsentForm
{
    public class ConsentFormEndPoint
    {
        private const string _route = "consent-form";
        private const string _tag = "Consent Form";

        private readonly CheckHardCopyDocumentSubmissionHandler _checkHardCopyDocumentSubmissionHandler;
        private readonly GetPeriodEntryHandler _getPeriodEntryHandler;
        private readonly GetStudentConsentFormHandler _getStudentConsentFormHandler;
        private readonly GetDocumentConsentFormHandler _getDocumentConsentFormHandler;

        public ConsentFormEndPoint(
           CheckHardCopyDocumentSubmissionHandler checkHardCopyDocumentSubmissionHandler, 
           GetPeriodEntryHandler getPeriodEntryHandler,
           GetStudentConsentFormHandler getStudentConsentFormHandler,
           GetDocumentConsentFormHandler getDocumentConsentFormHandler)
        {
            _checkHardCopyDocumentSubmissionHandler = checkHardCopyDocumentSubmissionHandler;
            _getPeriodEntryHandler = getPeriodEntryHandler;
            _getStudentConsentFormHandler = getStudentConsentFormHandler;
            _getDocumentConsentFormHandler = getDocumentConsentFormHandler;
        }

        [FunctionName(nameof(ConsentFormEndPoint.CheckHardCopyDocumentSubmission))]
        [OpenApiOperation(tags: _tag, Summary = "Check Hard Copy Document Submission")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CheckHardCopyDocumentSubmissionRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CheckHardCopyDocumentSubmissionRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CheckHardCopyDocumentSubmissionRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CheckHardCopyDocumentSubmissionRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CheckHardCopyDocumentSubmissionResult))]
        public Task<IActionResult> CheckHardCopyDocumentSubmission(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/check-hard-copy-document-submission")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _checkHardCopyDocumentSubmissionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ConsentFormEndPoint.GetPeriodEntry))]
        [OpenApiOperation(tags: _tag, Summary = "Get Entry Period for Concern Form")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPeriodEntryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPeriodEntryRequest.IdParent), In = ParameterLocation.Query, Required = true)]        
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetPeriodEntryResult>))]
        public Task<IActionResult> GetPeriodEntry(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/entry-period")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getPeriodEntryHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ConsentFormEndPoint.GetStudentConsentForm))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student ConsentForm Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentConsentFormRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentConsentFormRequest.Semester), In = ParameterLocation.Query, Required = true)] 
        [OpenApiParameter(nameof(GetStudentConsentFormRequest.IdParent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentConsentFormResult>))]
        public Task<IActionResult> GetStudentConsentForm(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/all-student-status")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getStudentConsentFormHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ConsentFormEndPoint.GetDocumentConsentForm))]
        [OpenApiOperation(tags: _tag, Summary = "Get Document Consent Form HardCopy")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDocumentConsentFormRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDocumentConsentFormResult))]
        public Task<IActionResult> GetDocumentConsentForm(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-consent-form-doc")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getDocumentConsentFormHandler.Execute(req, cancellationToken);
        }
    }
}
