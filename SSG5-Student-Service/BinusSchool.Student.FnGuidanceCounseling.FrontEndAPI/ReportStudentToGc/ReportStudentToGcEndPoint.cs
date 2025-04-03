using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class ReportStudentToGcEndPoint
    {
        private const string _route = "guidance-counseling/report-student-to-gc";
        private const string _tag = "Report Student To GC";

        private readonly ReportStudentToGcHandler _reportStudentToGcHandler;
        public ReportStudentToGcEndPoint(ReportStudentToGcHandler reportStudentToGcHandler)
        {
            _reportStudentToGcHandler = reportStudentToGcHandler;
        }

        #region report-student-to-gc
        [FunctionName(nameof(ReportStudentToGcEndPoint.GetReportStudentToGcByTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get Report Student To GC")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetReportStudentToGcRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetReportStudentToGcRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetReportStudentToGcRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetReportStudentToGcRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetReportStudentToGcRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetReportStudentToGcRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetReportStudentToGcRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetReportStudentToGcResult[]))]
        public Task<IActionResult> GetReportStudentToGcByTeacher(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _reportStudentToGcHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReportStudentToGcEndPoint.AddReportStudentToGc))]
        [OpenApiOperation(tags: _tag, Summary = "Add Report Student To GC")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddReportStudentToGcRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddReportStudentToGc(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        [Queue("notification-gc-guidancecounseling")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _reportStudentToGcHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ReportStudentToGcEndPoint.GetDetailReportStudentToGc))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Report Student To GC")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailReportStudentToGcResult))]
        public Task<IActionResult> GetDetailReportStudentToGc(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
         string id,
      CancellationToken cancellationToken)
        {
            return _reportStudentToGcHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(ReportStudentToGcEndPoint.UpdateReportStudentToGc))]
        [OpenApiOperation(tags: _tag, Summary = "Update Report Student To GC")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateReportStudentToGcRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateReportStudentToGc(
         [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _reportStudentToGcHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReportStudentToGcEndPoint.DeleteReportStudentToGc))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Class Diary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteReportStudentToGcRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteReportStudentToGc(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        [Queue("notification-gc-guidancecounseling")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteReportStudentToGcHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }


        #endregion

        #region student-gc-report
        [FunctionName(nameof(ReportStudentToGcEndPoint.GetReportStudentToGcByConsollor))]
        [OpenApiOperation(tags: _tag, Summary = "Get Report Student To GC By Counselor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetReportStudentToGcRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetReportStudentToGcRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetReportStudentToGcRequest.StartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetReportStudentToGcRequest.EndDate), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetReportStudentToGcByCounsolorResult[]))]
        public Task<IActionResult> GetReportStudentToGcByConsollor(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-counselor")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ReportStudentToGcByCounselorHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReportStudentToGcEndPoint.ReadStudentToGc))]
        [OpenApiOperation(tags: _tag, Summary = "Read Report Student To GC")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ReadStudentToGcRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> ReadStudentToGc(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-by-counselor")] HttpRequest req,
        [Queue("notification-enn")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ReadStudentToGcHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
        #endregion

        #region summary-counselling
        [FunctionName(nameof(ReportStudentToGcEndPoint.GetReportStudentToGcByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Report Student To GC By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetReportStudentToGcByStudentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetReportStudentToGcByStudentRequest.IdUserStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetReportStudentToGcByStudentResult[]))]
        public Task<IActionResult> GetReportStudentToGcByStudent(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-student")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetReportStudentToGcByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReportStudentToGcEndPoint.GetCounselingServicesEntryByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Counseling Service Entry By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCounselingServicesEntryByStudentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCounselingServicesEntryByStudentRequest.IdUserStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCounselingServicesEntryByStudentRequest.IdConselingCategory), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCounselingServicesEntryByStudentResult[]))]
        public Task<IActionResult> GetCounselingServicesEntryByStudent(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-counseling-entry")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCounselingServicesEntryByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReportStudentToGcEndPoint.GetSummaryCounseling))]
        [OpenApiOperation(tags: _tag, Summary = "Get Summary Counsoller")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSummaryCounselingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryCounselingRequest.IdUserCounselor), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryCounselingRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryCounselingRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryCounselingRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSummaryCounselingRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSummaryCounselingResult[]))]
        public Task<IActionResult> GetSummaryCounseling(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-summary-counseling")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSummaryCounselingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReportStudentToGcEndPoint.GetWizardCounseling))]
        [OpenApiOperation(tags: _tag, Summary = "Get Wizard Counsoller")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetWizardCounselingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetWizardCounselingRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetWizardCounselingRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetWizardCounselingRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetWizardCounselingRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetWizardCounselingRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetWizardCounselingResult[]))]
        public Task<IActionResult> GetWizardCounseling(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-wizard-counseling")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetWizardCounselingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ReportStudentToGcEndPoint.GetExcelCounselingServicesEntryByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Excel Counseling Services Entry")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetExcelCounselingServicesEntryByStudentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetExcelCounselingServicesEntryByStudent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-excel-counselor-services-entry")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetExcelCounselingServicesEntryByStudentHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
        [FunctionName(nameof(ReportStudentToGcEndPoint.GetExcelReportStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Excel Report Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetExcelReportStudentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetExcelReportStudent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-excel-report-student")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetExcelReportStudentHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ReportStudentToGcEndPoint.GetGcAccess))]
        [OpenApiOperation(tags: _tag, Summary = "Get Gc Access")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetGcAccessRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetGcAccessRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetWizardCounselingResult[]))]
        public Task<IActionResult> GetGcAccess(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-access")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGcAccessHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

    }
}
