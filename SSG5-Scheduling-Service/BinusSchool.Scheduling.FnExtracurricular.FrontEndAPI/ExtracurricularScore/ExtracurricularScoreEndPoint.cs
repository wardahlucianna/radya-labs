using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class ExtracurricularScoreEndPoint
    {
        private const string _route = "extracurricular/score";
        private const string _tag = "Extracurricular Score";

        private readonly GetUnSubmittedScoreHandler _getunsubmittedscorehandler;
        private readonly GetExtracurricularScoreEntryHandler _getextracurricularscoreentryhandler;
        private readonly GetExtracurricularStudentScoreHandler _getextracurricularstudentscorehandler;
        private readonly UpdateExtracurricularStudentScoreHandler _updateextracurricularstudentscorehandler;
        private readonly UpdateExtracurricularScoreLegendHandler _updateextracurricularscorelegendhandler;
        private readonly GetExtracurricularScoreLegendHandler _getextracurricularscorelegendhandler;
        private readonly ExportExcelExtracurricularStudentScoreHandler _exportexcelextracurricularstudentscorehandler;
        private readonly UpdateExtracurricularScoreLegendHandler2 _updateExtracurricularScoreLegendHandler2;
        private readonly GetExtracurricularScoreLegendHandler2 _getExtracurricularScoreLegendHandler2;
        private readonly GetExtracurricularScoreEntryHandler2 _getExtracurricularScoreEntryHandler2;
        private readonly ExportExcelUnSubmittedScoreHandler _exportExcelUnSubmittedScoreHandler;


        public ExtracurricularScoreEndPoint(
           GetUnSubmittedScoreHandler getunsubmittedscorehandler,
           GetExtracurricularScoreEntryHandler getextracurricularscoreentryhandler,
           GetExtracurricularStudentScoreHandler getextracurricularstudentscorehandler,
           UpdateExtracurricularStudentScoreHandler updateextracurricularstudentscorehandler,
           UpdateExtracurricularScoreLegendHandler updateextracurricularscorelegendhandler,
           GetExtracurricularScoreLegendHandler getextracurricularscorelegendhandler,
           ExportExcelExtracurricularStudentScoreHandler exportexcelextracurricularstudentscorehandler,
           UpdateExtracurricularScoreLegendHandler2 updateExtracurricularScoreLegendHandler2,
           GetExtracurricularScoreLegendHandler2 getExtracurricularScoreLegendHandler2,
           GetExtracurricularScoreEntryHandler2 getExtracurricularScoreEntryHandler2,
           ExportExcelUnSubmittedScoreHandler exportExcelUnSubmittedScoreHandler)
        {
            _getunsubmittedscorehandler = getunsubmittedscorehandler;
            _getextracurricularscoreentryhandler = getextracurricularscoreentryhandler;
            _getextracurricularstudentscorehandler = getextracurricularstudentscorehandler;
            _getextracurricularscorelegendhandler = getextracurricularscorelegendhandler;
            _updateextracurricularstudentscorehandler = updateextracurricularstudentscorehandler;
            _updateextracurricularscorelegendhandler = updateextracurricularscorelegendhandler;
            _exportexcelextracurricularstudentscorehandler = exportexcelextracurricularstudentscorehandler;
            _updateExtracurricularScoreLegendHandler2 = updateExtracurricularScoreLegendHandler2;
            _getExtracurricularScoreLegendHandler2 = getExtracurricularScoreLegendHandler2;
            _getExtracurricularScoreEntryHandler2 = getExtracurricularScoreEntryHandler2;
            _exportExcelUnSubmittedScoreHandler = exportExcelUnSubmittedScoreHandler;
        }


        [FunctionName(nameof(ExtracurricularScoreEndPoint.GetUnSubmittedScore))]
        [OpenApiOperation(tags: _tag, Summary = "Get UnSubmitted Extracurricular Student Score")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        // [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        // [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        // [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUnSubmittedScoreRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUnSubmittedScoreRequest.Semester), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetUnSubmittedScoreRequest.IdBinusian), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnSubmittedScoreResult))]
        public Task<IActionResult> GetUnSubmittedScore(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unsubmitted")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _getunsubmittedscorehandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ExtracurricularScoreEndPoint.GetExtracurricularScoreEntry))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular and Legends in Score Entry Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExtracurricularRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExtracurricularRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExtracurricularRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]        
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetExtracurricularResult))]
        public Task<IActionResult> GetExtracurricularScoreEntry(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/extracurricular-score-entry")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _getextracurricularscoreentryhandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularScoreEndPoint.GetExtracurricularStudentScore))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Score by IdExtracurricular")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]      
        [OpenApiParameter(nameof(GetExtracurricularStudentScoreRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExtracurricularStudentScoreRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetExtracurricularStudentScoreRequest.IdExtracurricular), In = ParameterLocation.Query, Required = true)]  
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetExtracurricularStudentScoreResult))]
        public Task<IActionResult> GetExtracurricularStudentScore(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student-score")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getextracurricularstudentscorehandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularScoreEndPoint.GetExtracurricularScoreLegend))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Score Legend")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExtracurricularScoreLegendRequest.IdSchool), In = ParameterLocation.Query, Required = true)]       
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetExtracurricularScoreLegendResult>))]
        public Task<IActionResult> GetExtracurricularScoreLegend(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-score-legend")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getextracurricularscorelegendhandler.Execute(req, cancellationToken);

        }    

        [FunctionName(nameof(ExtracurricularScoreEndPoint.UpdateExtracurricularStudentScore))]
        [OpenApiOperation(tags: _tag, Summary = "Update Extracurricular Student Score")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateExtracurricularStudentScoreRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateExtracurricularStudentScore(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-student-score")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _updateextracurricularstudentscorehandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularScoreEndPoint.UpdateExtracurricularScoreLegend))]
        [OpenApiOperation(tags: _tag, Summary = "Update Extracurricular Score Legend")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateExtracurricularScoreLegendRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateExtracurricularScoreLegend(
         [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-score-legend")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _updateextracurricularscorelegendhandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularScoreEndPoint.ExportExcelExtracurricularStudentScore))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Extracurricular Student Score")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]            
        [OpenApiRequestBody("application/json", typeof(GetExtracurricularStudentScoreRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelExtracurricularStudentScore(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-excel-student-score")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _exportexcelextracurricularstudentscorehandler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(ExtracurricularScoreEndPoint.UpdateExtracurricularScoreLegend2))]
        [OpenApiOperation(tags: _tag, Summary = "Update Extracurricular Score Legend v2 with Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateExtracurricularScoreLegendRequest2))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateExtracurricularScoreLegend2(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-score-legend-v2")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _updateExtracurricularScoreLegendHandler2.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ExtracurricularScoreEndPoint.GetExtracurricularScoreLegend2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular Score Legend  v2 with Category")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExtracurricularScoreLegendRequest2.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetExtracurricularScoreLegendResult2>))]
        public Task<IActionResult> GetExtracurricularScoreLegend2(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-score-legend-v2")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getExtracurricularScoreLegendHandler2.Execute(req, cancellationToken);

        }

        [FunctionName(nameof(ExtracurricularScoreEndPoint.GetExtracurricularScoreEntry2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular and Legends in Score Entry Period v2 with Scorelegend mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExtracurricularRequest2.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExtracurricularRequest2.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExtracurricularRequest2.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetExtracurricularResult2))]
        public Task<IActionResult> GetExtracurricularScoreEntry2(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/extracurricular-score-entry-v2")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getExtracurricularScoreEntryHandler2.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ExtracurricularScoreEndPoint.ExportExcelUnSubmittedScore))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Unsubmitted Score")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetUnSubmittedScoreRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelUnSubmittedScore(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-excel-unsubmiited-score")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _exportExcelUnSubmittedScoreHandler.Execute(req, cancellationToken, false);
        }



    }
}
