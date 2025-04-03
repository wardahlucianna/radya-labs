using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class CreativityActivityServiceEndpoint
    {
        private const string _route = "student/creativity-activity-service";
        private const string _tag = "Creativity Activity Service";

        #region Experience Student
        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetListExperience))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Experience")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListExperienceRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExperienceRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExperienceRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListExperienceRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListExperienceRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListExperienceRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetListExperienceRequest.IsCASCoordinator), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListExperienceRequest.ViewAs), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListExperienceResult))]
        public Task<IActionResult> GetListExperience(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-experience")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListExperienceHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.DetailExperience))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Experience")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DetailExperienceRequest.IdExperience), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(DetailExperienceRequest.IdUser), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(DetailExperienceRequest.Role), In = ParameterLocation.Query,Required =true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailExperienceResult))]
        public Task<IActionResult> DetailExperience(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail-experience")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailExperienceHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.AddExperience))]
        [OpenApiOperation(tags: _tag, Summary = "Add Experience")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddExperienceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddExperience(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-experience")] HttpRequest req,
             [Queue("notification-cas")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddExperienceHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.UpdateExperience))]
        [OpenApiOperation(tags: _tag, Summary = "Update Experience")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateExperienceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateExperience(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-experience")] HttpRequest req,
             [Queue("notification-cas")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateExperienceHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetTimeline))]
        [OpenApiOperation(tags: _tag, Summary = "Get Timeline")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTimelineRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTimelineRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetTimelineRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTimelineRequest.ViewAs), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetTimelineRequest.IsCASCoordinator), In = ParameterLocation.Query, Type = typeof(bool))]

        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTimelineResult))]
        public Task<IActionResult> GetTimeline(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-timeline")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTimelineHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.UpdateStatusExperience))]
        [OpenApiOperation(tags: _tag, Summary = "Update Status Experience")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStatusExperienceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateStatusExperience(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-status-experience")] HttpRequest req,
             [Queue("notification-cas")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateStatusExperienceHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.UpdateOverallProgressStatusStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Update Overall Progress Status Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateOverallProgressStatusStudentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateOverallProgressStatusStudent(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-overall-progress-status-student")] HttpRequest req,
             [Queue("notification-cas")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateOverallProgressStatusStudentHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.DeleteExperience))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Experience")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteExperienceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteExperience(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-experience")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteExperienceHandler>();
            return handler.Execute(req, cancellationToken);
        }

        #endregion

        #region Learning Outcome
        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetListLearningOutcome))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Learning Outcome")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListLearningOutcomeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListLearningOutcomeResult))]
        public Task<IActionResult> GetListLearningOutcome(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-learning-outcome")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListLearningOutcomeHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        #endregion

        #region Experience Supervisor
        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetAcademicYearAndGradeBySupervisor))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAcademicYearAndGradeBySupervisorRequest.IdUser), In = ParameterLocation.Query,Required =true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAcademicYearAndGradeBySupervisorResult))]
        public Task<IActionResult> GetAcademicYearAndGradeBySupervisor(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-ay-grade-by-supervisor")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAcademicYearAndGradeBySupervisorHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetListExperienceBySupervisor))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListExperienceRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExperienceRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExperienceRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListExperienceBySupervisorRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListExperienceBySupervisorRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListExperienceBySupervisorRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListExperienceBySupervisorResult))]
        public Task<IActionResult> GetListExperienceBySupervisor(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-experience-by-supervisor")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListExperienceBySupervisorHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetListStudentBySupervisor))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListExperienceRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExperienceRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListExperienceRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListExperienceRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListStudentBySupervisorRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListStudentBySupervisorRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListStudentBySupervisorRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentBySupervisorRequest.Status), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListStudentBySupervisorResult))]
        public Task<IActionResult> GetListStudentBySupervisor(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-student-by-supervisor")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListStudentBySupervisorHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetTimelineBySupervisor))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTimelineBySupervisorRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTimelineBySupervisorRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTimelineBySupervisorRequest.IsCASCoordinator), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetTimelineBySupervisorRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTimelineBySupervisorResult))]
        public Task<IActionResult> GetTimelineBySupervisor(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-timeline-by-supervisor")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTimelineBySupervisorHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
        #endregion

        #region Student Information
        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetAcademicYearForStudentExperience))]
        [OpenApiOperation(tags: _tag, Summary = "Get Academic Year For Student Experience")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAcademicYearForStudentExperienceRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAcademicYearForStudentExperienceResult))]
        public Task<IActionResult> GetAcademicYearForStudentExperience(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-ay-for-student-experience")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAcademicYearForStudentExperienceHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetStudentInformationByAcademicYear))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Information By Academic Year")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentInformationByAcademicYearRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentInformationByAcademicYearRequest.IdAcademicYears), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentInformationByAcademicYearResult))]
        public Task<IActionResult> GetStudentInformationByAcademicYear(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-information-by-ay")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentInformationByAcademicYearHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        #endregion

        #region Evidences
        [FunctionName(nameof(CreativityActivityServiceEndpoint.AddEvidences))]
        [OpenApiOperation(tags: _tag, Summary = "Add Evidences")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddEvidencesRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddEvidences(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-evidences")] HttpRequest req,
             [Queue("notification-cas")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddEvidencesHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.UpdateEvidences))]
        [OpenApiOperation(tags: _tag, Summary = "Update Evidences")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateEvidencesRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateEvidences(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-evidences")] HttpRequest req,
             [Queue("notification-cas")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateEvidencesHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.DetailEvidences))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Evidences")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DetailEvidencesRequest.IdEvidences), In = ParameterLocation.Query,Required =true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailEvidencesResult))]
        public Task<IActionResult> DetailEvidences(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail-evidences")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailEvidencesHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetListEvidences))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Evidences")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListEvidencesRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListEvidencesRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListEvidencesRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListEvidencesRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListEvidencesRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListEvidencesRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListEvidencesRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListEvidencesRequest.IdExperience), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListEvidencesRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListEvidencesResult))]
        public Task<IActionResult> GetListEvidences(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-evidences")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListEvidencesHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.DeleteEvidences))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Evidences")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteEvidencesRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteEvidences(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-evidences")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteEvidencesHandler>();
            return handler.Execute(req, cancellationToken);
        }

        #endregion

        #region Download
        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetTimelineToPdf))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetTimelineToPdfRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTimelineToPdfRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTimelineToPdfRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetTimelineToPdfRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetTimelineBySupervisorResult))]
        public Task<IActionResult> GetTimelineToPdf(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-timeline-to-pdf")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTimelineToPdfHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetExperienceToPdf))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetExperienceToPdfRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExperienceToPdfRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExperienceToPdfRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetExperienceToPdfRequest.IsComment), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetExperienceToPdfRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetExperienceResult[]))]
        public Task<IActionResult> GetExperienceToPdf(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-experience-to-pdf")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetExperienceToPdfHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
        
        [FunctionName(nameof(CasRequestDownload))]
        [OpenApiOperation(tags: _tag, Summary = "Request cas download")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CasExperienceDownloadRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> CasRequestDownload(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/cas-request-download")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<RequestDownloadHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(EmailDownload))]
        [OpenApiOperation(tags: _tag, Summary = "Email download")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(EmailDownloadResult))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> EmailDownload(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/email-download")] HttpRequest req,
            [Queue("notification-cas")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<EmailDownloadHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
        #endregion

        #region Comment Evidences
        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetListCommentEvidences))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Comment Evidences")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListCommentEvidencesRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCommentEvidencesRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCommentEvidencesRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListCommentEvidencesRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListCommentEvidencesRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCommentEvidencesRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListCommentEvidencesRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListCommentEvidencesRequest.IdEvidences), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListCommentEvidencesRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListCommentEvidencesRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListCommentEvidencesResult))]
        public Task<IActionResult> GetListCommentEvidences(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-comment-evidences")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListCommentEvidencesHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.SaveCommentEvidences))]
        [OpenApiOperation(tags: _tag, Summary = "Save Comment Evidences")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveCommentEvidencesRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> SaveCommentEvidences(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-comment-evidences")] HttpRequest req,
             [Queue("notification-cas")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SaveCommentEvidencesHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(CreativityActivityServiceEndpoint.DeleteCommentEvidences))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Comment Evidences")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteCommentEvidencesRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteCommentEvidences(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-comment-evidences")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteCommentEvidencesHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region CAS
        [FunctionName(nameof(CreativityActivityServiceEndpoint.GetListStudentByCAS))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Student By CAS")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.ViewAs), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListStudentByCASRequest.StatusOverallExperienceStudent), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListStudentByCASResult))]
        public Task<IActionResult> GetListStudentByCAS(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-student-by-cas")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListStudentByCASHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
        
        #endregion
    }
}
