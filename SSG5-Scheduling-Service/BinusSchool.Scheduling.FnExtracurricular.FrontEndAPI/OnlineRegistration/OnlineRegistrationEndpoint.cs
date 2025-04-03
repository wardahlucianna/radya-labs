using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnExtracurricular.OnlineRegistration
{
    public class OnlineRegistrationEndpoint
    {
        private const string _route = "scp-online-registration";
        private const string _tag = "SCP Online Registration";

        private readonly GetRegistrationDetailHandler _getRegistrationDetailHandler;
        private readonly GetExtracurricularListByStudentHandler _getExtracurricularListByStudentHandler;
        private readonly SaveStudentExtracurricularHandler _saveStudentExtracurricularHandler;
        private readonly GetActiveStudentsGradeByStudentHandler _getActiveStudentsGradeByStudentHandler;
        private readonly GetSupportingDucumentByStudentHandler _getsupportdocummentbystudenthandler; 

        public OnlineRegistrationEndpoint(
            GetRegistrationDetailHandler getRegistrationDetailHandler,
            GetExtracurricularListByStudentHandler getExtracurricularListByStudentHandler,
            SaveStudentExtracurricularHandler saveStudentExtracurricularHandler,
            GetActiveStudentsGradeByStudentHandler getActiveStudentsGradeByStudentHandler,
            GetSupportingDucumentByStudentHandler getsupportdocummentbystudenthandler)
        {
            _getRegistrationDetailHandler = getRegistrationDetailHandler;
            _getExtracurricularListByStudentHandler = getExtracurricularListByStudentHandler;
            _saveStudentExtracurricularHandler = saveStudentExtracurricularHandler;
            _getActiveStudentsGradeByStudentHandler = getActiveStudentsGradeByStudentHandler;
            _getsupportdocummentbystudenthandler = getsupportdocummentbystudenthandler;
        }

        [FunctionName(nameof(OnlineRegistrationEndpoint.GetRegistrationDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Registration Information Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetRegistrationDetailRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetRegistrationDetailResult))]
        public Task<IActionResult> GetRegistrationDetail([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-registration-detail")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getRegistrationDetailHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(OnlineRegistrationEndpoint.GetExtracurricularListByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Extracurricular List By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetExtracurricularListByStudentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetExtracurricularListByStudentResult))]
        public Task<IActionResult> GetExtracurricularListByStudent([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-extracurricular-list-by-student")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getExtracurricularListByStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(OnlineRegistrationEndpoint.SaveStudentExtracurricular))]
        [OpenApiOperation(tags: _tag, Summary = "Save Student Extracurricular")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveStudentExtracurricularRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SaveStudentExtracurricularResult))]
        public Task<IActionResult> SaveStudentExtracurricular([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-student-extracurricular")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveStudentExtracurricularHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(OnlineRegistrationEndpoint.GetActiveStudentGradeByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Active Student Grade By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetActiveStudentsGradeByStudentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetActiveStudentsGradeByStudentResult))]
        public Task<IActionResult> GetActiveStudentGradeByStudent([HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-active-grade-by-students")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getActiveStudentsGradeByStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(OnlineRegistrationEndpoint.GetSupportingDucumentByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Supporting Ducument By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]       
        [OpenApiParameter(nameof(GetSupportingDucumentByStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSupportingDucumentByStudentRequest.ShowToStudent), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSupportingDucumentByStudentResult>))]
        public Task<IActionResult> GetSupportingDucumentByStudent([HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-support-doc")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getsupportdocummentbystudenthandler.Execute(req, cancellationToken);
        }


    }
}
