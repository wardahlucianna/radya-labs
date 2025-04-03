using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class ClassDiaryEndPoint
    {
        private const string _route = "schedule/class-diary";
        private const string _tag = "Class Diary";

        private readonly ClassDiaryHandler _classDiaryHandler;
        private readonly ClassDiaryDeletionApprovalHandler _classDiaryDeletionApprovalHandler;
        public ClassDiaryEndPoint(ClassDiaryHandler ClassDiaryHandler, ClassDiaryDeletionApprovalHandler ClassDiaryDeletionApprovalHandler)
        {
            _classDiaryHandler = ClassDiaryHandler;
            _classDiaryDeletionApprovalHandler = ClassDiaryDeletionApprovalHandler;
        }

        #region class-diary
        [FunctionName(nameof(ClassDiaryEndPoint.GetClassDiary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Class Diary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.AcademicYearId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.IsHeadRole), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.GradeId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.SubjectId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.HomeroomId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.LessonId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.ClassDiaryDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.ClassDiaryTypeSettingId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.ClassDiaryStatus), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryRequest.ClassId), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassDiaryResult[]))]
        public Task<IActionResult> GetClassDiary(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _classDiaryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryEndPoint.AddClassDiary))]
        [OpenApiOperation(tags: _tag, Summary = "Add Class Diary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddClassDiaryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddClassDiary(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
         [Queue("notification-cd-schedule")] ICollector<string> collector,
         CancellationToken cancellationToken)
        {
            return _classDiaryHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetDetailClassDiary))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Class Diary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailClassDiaryResult))]
        public Task<IActionResult> GetDetailClassDiary(
      [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
         string id,
      CancellationToken cancellationToken)
        {
            return _classDiaryHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(ClassDiaryEndPoint.UpdateClassDiary))]
        [OpenApiOperation(tags: _tag, Summary = "Update Class Diary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateClassDiaryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateClassDiary(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
        [Queue("notification-cd-schedule")] ICollector<string> collector,
         CancellationToken cancellationToken)
        {
            return _classDiaryHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ClassDiaryEndPoint.DeleteClassDiary))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Class Diary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteClassDiaryRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteClassDiary(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        [Queue("notification-cd-schedule")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteClassDiaryHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        //==================================================

        [FunctionName(nameof(ClassDiaryEndPoint.GetHomeroomByStudentClassDiary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Homeroom By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetHomeroomByStudentRequest.AcademicYearId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomByStudentRequest.GradeId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomByStudentRequest.SubjectId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomByStudentRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetHomeroomByStudentRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm[]))]
        public Task<IActionResult> GetHomeroomByStudentClassDiary(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-homeroom-by-student")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetHomeroomByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetsubjectClassDiary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Subject Class Diary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetSubjectClassDiaryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectClassDiaryRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSubjectClassDiaryRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLevelClassDiaryResult[]))]
        public Task<IActionResult> GetsubjectClassDiary(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-subject")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetSubjectClassDiaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetClassIdByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get ClassId By Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetClassIdByStudentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm[]))]
        public Task<IActionResult> GetClassIdByStudent(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/class-id-by-student")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClassIdByStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetClassIdByTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get ClassId By Teacher")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetClassIdByStudentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm[]))]
        public Task<IActionResult> GetClassIdByTeacher(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/class-id-by-Teacher")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClassIdByTeacherHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetStartAndEndDatePeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Get Start Date and End Date")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStartAndEndDatePeriodRequest.AcademicYearId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStartAndEndDatePeriodRequest.GradeId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStartAndEndDatePeriodRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStartAndEndDatePeriodResult))]
        public Task<IActionResult> GetStartAndEndDatePeriod(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-start-end-date-period")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStartAndEndDatePeriodHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetCheackingTypeSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Cheacking Type Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetCheackingTypeSettingRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CheckTypeFeedbackResult[]))]
        public Task<IActionResult> GetCheackingTypeSetting(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/checking-type-setting")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCheackingTypeSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ClassDiaryEndPoint.GetTypeSettingClassDiary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Type Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetTypeSettingClassDiaryRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm[]))]
        public Task<IActionResult> GetTypeSettingClassDiary(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/type-setting")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTypeSettingClassDiaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetClassDiaryHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Class Diary History")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassDiaryHistoryRequest.AcademicYearId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassDiaryHistoryRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassDiaryHistoryRequest.GradeId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryHistoryRequest.SubjectId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryHistoryRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryHistoryRequest.HomeroomId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryHistoryRequest.LessonId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryHistoryRequest.ClassDiaryDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryHistoryRequest.ClassDiaryTypeSettingId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryHistoryRequest.ClassDiaryStatus), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryHistoryRequest.RequestDate), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassDiaryHistoryResult[]))]
        public Task<IActionResult> GetClassDiaryHistory(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-history")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetClassDiaryHistoryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetUserTeacherDetailById))]
        [OpenApiOperation(tags: _tag, Summary = "Get User Teacher Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserTeacherDetailByIdRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserTeacherDetailByIdRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUserTeacherDetailByIdResult))]
        public Task<IActionResult> GetUserTeacherDetailById(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-user-teacher-detail")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUserTeacherDetailById>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetUserStudentDetailById))]
        [OpenApiOperation(tags: _tag, Summary = "Get User Student Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserTeacherDetailByIdRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserTeacherDetailByIdRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserTeacherDetailByIdRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUserStudentDetailByIdResult))]
        public Task<IActionResult> GetUserStudentDetailById(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-user-student-detail")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUserStudentDetailByIdHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetCreateAccessClassDiary))]
        [OpenApiOperation(tags: _tag, Summary = "Get Create Accsess Class Diary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCreateAccessClassDiaryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetCreateAccessClassDiaryRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCreateAccessClassDiaryResult))]
        public Task<IActionResult> GetCreateAccessClassDiary(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-access")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCreateAccessClassDiaryHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetCheackingClasslimit))]
        [OpenApiOperation(tags: _tag, Summary = "Get Cheacking Class limit")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetCheackingClassLimitRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CheckClassLimitClassDiaryResult[]))]
        public Task<IActionResult> GetCheackingClasslimit(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/checking-class-limit")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetCheackingClassLimitHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region class-diary-deletion-approval
        [FunctionName(nameof(ClassDiaryEndPoint.GetClassDiaryDeletionApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Get Class Diary Deletion Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.AcademicYearId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.UserId), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.GradeId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.SubjectId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.Semester), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.HomeroomId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.LessonId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.ClassDiaryDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.ClassDiaryTypeSettingId), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.ClassDiaryStatus), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.RequestBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetClassDiaryDeletionApprovalRequest.RequestDate), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetClassDiaryDeletionApprovalResult[]))]
        public Task<IActionResult> GetClassDiaryDeletionApproval(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-deletion-approval")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _classDiaryDeletionApprovalHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(ClassDiaryEndPoint.AddClassDiaryDeletionApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Add Class Diary Deletion")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddClassDiaryDeletionApprovalRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddClassDiaryDeletionApproval(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-deletion-approval")] HttpRequest req,
        [Queue("notification-cd-schedule")] ICollector<string> collector,
        CancellationToken cancellationToken)
        {
            return _classDiaryDeletionApprovalHandler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(ClassDiaryEndPoint.GetDetailClassDiaryDeletionApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Class Diary Deletion Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDetailClassDiaryDeletionApprovalResult))]
        public Task<IActionResult> GetDetailClassDiaryDeletionApproval(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-deletion-approval" + "/{id}")] HttpRequest req,
        string id,
        CancellationToken cancellationToken)
        {
            return _classDiaryDeletionApprovalHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        #endregion

    }
}
