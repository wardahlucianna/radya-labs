using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemerit;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using BinusSchool.Student.FnStudent.MeritDemeritTeacher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.MeritDemerit
{
    public class MeritDemeritTeacherEndPoint
    {
        private const string _route = "student/merit-demerit-teacher";
        private const string _tag = "Merit and Demerit Teacher";

        private readonly MeritDemeritTeacherFreezeHandler _freezeHandler;
        private readonly MeritDemeritTeacherHandler _meritDemeritTeacherHandler;
        private readonly ApprovalMeritDemeritTeacherHandler _approvalMeritDemeritTeacherHandler;
        //private readonly MeritDemeritLevelInfractionHandler _LevelInfractionHandler;
        //private readonly ScoreContinuationSettingHeandler _scoreContinuationHandler;
        private readonly MeritStudentHandler _meritStudentHandler;
        public MeritDemeritTeacherEndPoint(MeritDemeritTeacherFreezeHandler FreezeHandler, MeritDemeritTeacherHandler MeritDemeritTeacherHandler, ApprovalMeritDemeritTeacherHandler ApprovalMeritDemeritTeacherHandler, MeritStudentHandler MeritStudentHandler)
        {
            _freezeHandler = FreezeHandler;
            _meritDemeritTeacherHandler = MeritDemeritTeacherHandler;
            _approvalMeritDemeritTeacherHandler = ApprovalMeritDemeritTeacherHandler;
            _meritStudentHandler = MeritStudentHandler;
        }

        #region freeze
        [FunctionName(nameof(MeritDemeritTeacherEndPoint.UpdateFreeze))]
        [OpenApiOperation(tags: _tag, Summary = "Update Freeze")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateFreezeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateFreeze(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/freeze")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _freezeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetFreeze))]
        [OpenApiOperation(tags: _tag, Summary = "Get Freeze")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetFreezeRequest.IdAcademiYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetFreezeRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetFreezeRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetFreezeRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetFreezeRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetFreezeRequest.IsFreeze), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetFreezeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetFreezeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetFreezeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetFreezeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetFreezeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetFreezeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetFreezeResult[]))]
        public Task<IActionResult> GetFreeze(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/freeze")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _freezeHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region EntryMeritDemerit
        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetEntryMeritDemeritDisipline))]
        [OpenApiOperation(tags: _tag, Summary = "Get Merit Demerit Disipline")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritDisiplineRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritDisiplineRequest.Idlevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritDisiplineRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritDisiplineRequest.Category), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritDisiplineRequest.IdLevelInfraction), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEntryMeritDemeritDisiplineResult[]))]
        public Task<IActionResult> GetEntryMeritDemeritDisipline(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/disipline")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetEntryMeritDemeritDisiplineHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetEntryMeritDemeritStudentByFreeze))]
        [OpenApiOperation(tags: _tag, Summary = "Get Entry Merit Demerit Student By Freeze")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.Date), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEntryMeritDemeritStudentByFreezeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEntryMeritDemeritStudentByFreezeResult[]))]
        public Task<IActionResult> GetEntryMeritDemeritStudentByFreeze(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/student-by-freeze")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetEntryMeritDemeritStudentByFreezeHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetExsisEntryMeritDemeritTeacherById))]
        [OpenApiOperation(tags: _tag, Summary = "Add Entry Merit Demerit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetExsisEntryMeritDemeritTeacherByIdRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetExsisEntryMeritDemeritTeacherByIdResult[]))]
        public Task<IActionResult> GetExsisEntryMeritDemeritTeacherById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/entry-exsis")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetExsisEntryMeritDemeritTeacherByIdHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.AddMeritDemeritTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Add Entry Merit Demerit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMeritDemeritTeacherRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddMeritDemeritTeacherResult[]))]
        public Task<IActionResult> AddMeritDemeritTeacher(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/entry")] HttpRequest req,
        [Queue("notification-ds-student")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _meritDemeritTeacherHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetDetailEntryMeritDemerit))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Entry Merit and Demerit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetDetailEntryMeritDemeritRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailEntryMeritDemeritResult))]
        public Task<IActionResult> GetDetailEntryMeritDemerit(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/entry-detail")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailEntryMeritDemeritHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetMeritDemeritTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get Entry Merit Demerit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.PositionCode), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.ScoreSetting), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMeritDemeritTeacherRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMeritDemeritTeacherResult[]))]
        public Task<IActionResult> GetMeritDemeritTeacher(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/entry")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _meritDemeritTeacherHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetDetailMeritTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Entry Merit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdHomeroomStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailMeritTeacherResult[]))]
        public Task<IActionResult> GetDetailMeritTeacher(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/entry-merit-detail")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailEntryMeritHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetDetailMeritTeacherV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Entry Merit V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.IdHomeroomStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherV2Request.Type), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailMeritTeacherV2Result[]))]
        public Task<IActionResult> GetDetailMeritTeacherV2(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/entry-merit-detail-v2")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailEntryMeritV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetDetailDemeritTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Entry Demerit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdHomeroomStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetDetailMeritTeacherRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailDemeritTeacherResult[]))]
        public Task<IActionResult> GetDetailDemeritTeacher(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/entry-demerit-detail")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailEntryDemeritHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.DeleteEntryMeritTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Delet Entry Merit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteEntryMeritDemeritTeacherRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteEntryMeritTeacher(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/entry-merit")] HttpRequest req,
        [Queue("notification-ds-student")]ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteEntryMeritHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        
        [FunctionName(nameof(MeritDemeritTeacherEndPoint.DeleteEntryDemeritTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Delet Entry Demerit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteEntryMeritDemeritTeacherRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteEntryDemeritTeacher(
       [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/entry-demerit")] HttpRequest req,
       [Queue("notification-ds-student")] ICollector<string> collector,
       CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteEntryDemeritHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }


        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetDownloadTeacherMeritDemeritTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get History Approval Merit Demerit Teacher")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetDownloadTeacherMeritDemeritTeacherRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetDownloadTeacherMeritDemeritTeacher(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/download-excel")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadExcelMeritDemeritTeacherHandler>();
            return handler.Execute(req, cancellationToken,false);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.DownloadMeritAndAchievement))]
        [OpenApiOperation(tags: _tag, Summary = "Download Merit And Achievement")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetDetailMeritTeacherV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DownloadMeritAndAchievement(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/download-merit-achievement")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadMeritAndAchievementHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        #endregion

        #region Approval Merit Demerit
        [FunctionName(nameof(MeritDemeritTeacherEndPoint.ApprovalMeritDemeritTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Approval Merit Demerit Teacher")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ApprovalMeritDemeritTeacherRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> ApprovalMeritDemeritTeacher(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/approval")] HttpRequest req,
        [Queue("notification-ds-student")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _approvalMeritDemeritTeacherHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.DeclineMeritDemeritTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Decline Merit Demerit Teacher")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ApprovalMeritDemeritTeacherRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeclineMeritDemeritTeacher(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/approval")] HttpRequest req,
        [Queue("notification-ds-student")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _approvalMeritDemeritTeacherHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetDetailApprovalMeritDemerit))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Approval Merit and Demerit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetDetailApprovalMeritDemeritRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailApprovalMeritDemeritResult))]
        public Task<IActionResult> GetDetailApprovalMeritDemerit(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/approval-detail")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailApprovalMeritDemeritHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetApprovalMeritDemeritTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get Approval Merit Demerit Teacher")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.IdAcademiYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.Status), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetApprovalMeritDemeritTeacherRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetApprovalMeritDemeritTeacherResult[]))]
        public Task<IActionResult> GetApprovalMeritDemeritTeacher(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/approval")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _approvalMeritDemeritTeacherHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region History Approval Merit Demerit
        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetHistoryApprovalMeritDemeritTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get History Approval Merit Demerit Teacher")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.Status), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetHistoryApprovalMeritDemeritTeacherRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHistoryApprovalMeritDemeritTeacherResult[]))]
        public Task<IActionResult> GetHistoryApprovalMeritDemeritTeacher(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/history-approval")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHistoryApprovalMeritDemeritTeacherHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Wizard
        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetWizardStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Wizard Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(WizardStudentHandlerRequest.IdUser), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(WizardStudentHandlerRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(WizardStudentHandlerResult))]
        public Task<IActionResult> GetWizardStudent(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/wizard-student")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<WizardStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetWizardStudentAchievement))]
        [OpenApiOperation(tags: _tag, Summary = "Wizard Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetWizardStudentAchievementRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetWizardStudentAchievementRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetWizardStudentAchievementRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<GetWizardStudentAchievementResult>[]))]
        public Task<IActionResult> GetWizardStudentAchievement(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/wizard-student-Achievement")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetWizardStudentAchievementHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetWizardStudentDetailMerit))]
        [OpenApiOperation(tags: _tag, Summary = "Wizard Detail Merit Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.IdStudent), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.IdAcademicYear), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Semester), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(WizardStudentDetailMeritResult[]))]
        public Task<IActionResult> GetWizardStudentDetailMerit(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/wizard-student-detail-merit")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<WizardStudentDetailMeritHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetWizardStudentDetailMeritV2))]
        [OpenApiOperation(tags: _tag, Summary = "Wizard Detail Merit Student V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(WizardStudentDetailMeritV2Result[]))]
        public Task<IActionResult> GetWizardStudentDetailMeritV2(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/wizard-student-detail-merit-v2")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<WizardStudentDetailMeritV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetWizardStudentDetailDemerit))]
        [OpenApiOperation(tags: _tag, Summary = "Wizard Detail Demerit Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardDetailStudentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(WizardStudentDetailDemeritResult[]))]
        public Task<IActionResult> GetWizardStudentDetailDemerit(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/wizard-student-detail-Demerit")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<WizardStudentDetailDemeritHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetWizardTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Wizard Teacher")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(WizardStudentHandlerRequest.IdUser), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(WizardStudentHandlerRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(WizardStudentHandlerRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardStudentHandlerRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardStudentHandlerRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(WizardStudentHandlerRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(WizardStudentHandlerRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(WizardStudentHandlerRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(WizardTeacherResult))]
        public Task<IActionResult> GetWizardTeacher(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/wizard-teacher")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<WizardTeacherHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetPointSystemByIdUser))]
        [OpenApiOperation(tags: _tag, Summary = "Wizard Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPointSystemByIdUserRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPointSystemByIdUserRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPointSystemByIdUserRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(bool))]
        public Task<IActionResult> GetPointSystemByIdUser(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/point-system-by-user")] HttpRequest req,
      CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetPointSystemByIdUserHandler>();
            return handler.Execute(req, cancellationToken);
        }

        #endregion

        #region Merit Student
        [FunctionName(nameof(MeritDemeritTeacherEndPoint.AddMeritStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Add Merit Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMeritStudentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddMeritDemeritTeacherResult[]))]
        public Task<IActionResult> AddMeritStudent(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/merit-student")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _meritStudentHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region CornJob Point
        [FunctionName(nameof(MeritDemeritTeacherEndPoint.ResetMeritDemeritPoint))]
        [OpenApiOperation(tags: _tag, Summary = "Reset Merit Demerit Point")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ResetMeritDemeritPointRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> ResetMeritDemeritPoint(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/reset-point")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ResetMeritDemeritPointHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.ResetMeritDemeritPointV2))]
        [OpenApiOperation(tags: _tag, Summary = "Reset Merit Demerit Point")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ResetMeritDemeritPointV2Request))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> ResetMeritDemeritPointV2(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/reset-point-v2")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ResetMeritDemeritPointV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.CalculateMeritDemeritPoint))]
        [OpenApiOperation(tags: _tag, Summary = "Calculate Merit Demerit Point semester 1 ke semester 2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CalculateMeritDemeritPointRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> CalculateMeritDemeritPoint(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/calculate-point")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CalculateMeritDemeritPointHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.CalculateMeritDemeritPointSmt1))]
        [OpenApiOperation(tags: _tag, Summary = "Calculate Merit Demerit Point semester 1")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CalculateMeritDemeritPointRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> CalculateMeritDemeritPointSmt1(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/calculate-point-smt1")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CalculateMeritDemeritPointSmt1Handler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Update Merit Demerit
        [FunctionName(nameof(MeritDemeritTeacherEndPoint.GetDetailEntryMeritDemeritById))]
        [OpenApiOperation(tags: _tag, Summary = "Get Detail Entry Merit By Id")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetDetailEntryMeritDemeritByIdRequest.Id), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetDetailEntryMeritDemeritByIdRequest.IsMerit), In = ParameterLocation.Query, Required = true, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailEntryMeritDemeritByIdResult))]
        public Task<IActionResult> GetDetailEntryMeritDemeritById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/entry-merit-detail-by-id")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetDetailEntryMeritDemeritByIdHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(MeritDemeritTeacherEndPoint.UpdateMeritDemeritById))]
        [OpenApiOperation(tags: _tag, Summary = "Update Merit demerit by Id")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMeritDemeritByIdRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateMeritDemeritById(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/entry-merit-by-id")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateMeritDemeritByIdHandler>();
            return handler.Execute(req, cancellationToken);
        }

        #endregion
    }
}
