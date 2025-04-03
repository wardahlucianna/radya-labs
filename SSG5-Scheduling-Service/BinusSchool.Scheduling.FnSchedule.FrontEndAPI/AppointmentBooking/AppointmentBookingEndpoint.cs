using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class AppointmentBookingEndpoint
    {
        private const string _route = "schedule/appointment-booking";
        private const string _tag = "Appointment Booking";

        private readonly AppointmentBookingParentHendler _appointmentBookingParentHendler;
        public AppointmentBookingEndpoint(AppointmentBookingParentHendler AppointmentBookingParentHendler)
        {
            _appointmentBookingParentHendler = AppointmentBookingParentHendler;
        }
        #region Appointment Booking Setting
        [FunctionName(nameof(AppointmentBookingEndpoint.GetEventParentAndStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Event Parent and Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventParentAndStudentRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetEventParentAndStudentRequest.IdEventType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetEventParentAndStudentRequest.StartDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetEventParentAndStudentRequest.EndDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEventParentAndStudentResult))]
        public Task<IActionResult> GetEventParentAndStudent(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-event-parent-and-student")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetEventParentAndStudentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetListInvitationBookingSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Invitation Booking Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListInvitationBookingSettingRequest.IdUser), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListInvitationBookingSettingRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListInvitationBookingSettingRequest.Status), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListInvitationBookingSettingRequest.InvitationStartDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListInvitationBookingSettingRequest.InvitationEndDate), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListInvitationBookingSettingRequest.IsMyInvitationBooking), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetEventParentAndStudentResult))]
        public Task<IActionResult> GetListInvitationBookingSetting(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-invitation-booking-setting")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListInvitationBookingSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.DetailInvitationBookingSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Invitation Booking Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DetailInvitationBookingSettingRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailInvitationBookingSettingResult))]
        public Task<IActionResult> DetailInvitationBookingSetting(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail-invitation-booking-setting")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailInvitationBookingSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetUserForUserVenue))]
        [OpenApiOperation(tags: _tag, Summary = "Get List User Teacher For User Venue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUserForUserVenueRequest.IdRole), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserForUserVenueRequest.CodePosition), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetUserForUserVenueRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserForUserVenueRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUserForUserVenueResult))]
        public Task<IActionResult> GetUserForUserVenue(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-user-for-user-venue")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUserForUserVenueHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetGradeByAppointmentDate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Grade By Appointment Date")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetGradeByAppointmentDateRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetGradeByAppointmentDateResult))]
        public Task<IActionResult> GetGradeByAppointmentDate(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-grade-by-appointment-date")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetGradeByAppointmentDateHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.CreateInvitationBookingSettingHandler))]
        [OpenApiOperation(tags: _tag, Summary = "Create Invitation Booking Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CreateInvitationBookingSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> CreateInvitationBookingSettingHandler(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/create-invitation-booking-setting")] HttpRequest req,
           [Queue("notification-app")] ICollector<string> collector,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<CreateInvitationBookingSettingHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }


        [FunctionName(nameof(AppointmentBookingEndpoint.UpdateInvitationBookingSettingVanueOnly))]
        [OpenApiOperation(tags: _tag, Summary = "Update Invitation Booking Setting Vanue")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateInvitationBookingSettingVanueOnlyRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateInvitationBookingSettingVanueOnly(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/update-vanue")] HttpRequest req,
           [Queue("notification-app")] ICollector<string> collector,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateInvitationBookingSettingVanueOnlyHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.DeleteInvitationBookingSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Invitation Booking Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteInvitationBookingSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteInvitationBookingSetting(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-invitation-booking-setting")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteInvitationBookingSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetListBreakSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Break Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListBreakSettingRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListBreakSettingResult))]
        public Task<IActionResult> GetListBreakSetting(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-break-setting")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListBreakSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.DetailBreakSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Break Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DetailBreakSettingRequest.IdInvitationBookingSettingBreak), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailBreakSettingResult))]
        public Task<IActionResult> DetailBreakSetting(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail-break-setting")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DetailBreakSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.AddBreakSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Add Break Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddBreakSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddBreakSetting(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-break-setting")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddBreakSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.UpdateBreakSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Update Break Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateBreakSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateBreakSetting(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-break-setting")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateBreakSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.DeleteBreakSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Break Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteBreakSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteBreakSetting(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-break-setting")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteBreakSettingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetListSchedulePreview))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Schedule Preview")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetSchedulePreviewRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetSchedulePreviewRequest.InvitationDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSchedulePreviewResult))]
        public Task<IActionResult> GetListSchedulePreview(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-schedule-preview")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListSchedulePreviewHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetListTeacherByInvitation))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Teacher By Invitation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListTeacherByInvitationRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListTeacherByInvitationResult))]
        public Task<IActionResult> GetListTeacherByInvitation(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-teacher-by-invitation")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListTeacherByInvitationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetListRecap))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Recap")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetListRecapRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListRecapRequest.IdUserTeacher), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListRecapRequest.Status), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListRecapResult))]
        public Task<IActionResult> GetListRecap(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-recap")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.DownloadListRecap))]
        [OpenApiOperation(tags: _tag, Summary = "Download Recap")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(DownloadListRecapRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(DownloadListRecapRequest.IdUserTeacher), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(DownloadListRecapRequest.Status), In = ParameterLocation.Query)]
        public Task<IActionResult> DownloadListRecap(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/download-list-recap")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DownloadRecapHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetListTimeFromQuotaDuration))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Time From Quota Duration")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListTimeFromQuotaDurationRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListTimeFromQuotaDurationRequest.DateInvitation), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListTimeFromQuotaDurationRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListTimeFromQuotaDurationResult))]
        public Task<IActionResult> GetListTimeFromQuotaDuration(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-time-from-quota-duration")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListTimeFromQuotaDurationHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetUserByRolePositionExcludeSubject))]
        [OpenApiOperation(tags: _tag, Summary = "Get User By Role Position Exclude Subject")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetUserByRolePositionExcludeSubjectRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetUserByRolePositionExcludeSubjectResult>))]
        public Task<IActionResult> GetUserByRolePositionExcludeSubject(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-user-by-role-position-exclude-subject")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUserByRolePositionExcludeSubjectHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region ApproitmentBookingParent
        [FunctionName(nameof(AppointmentBookingEndpoint.GetAppointmentBookingParent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Appointment Booking Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetAppointmentBookingParentRequest.Role), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAppointmentBookingParentRequest.Date), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAppointmentBookingParentRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAppointmentBookingParentResult[]))]
        public Task<IActionResult> GetAppointmentBookingParent(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-parent")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _appointmentBookingParentHendler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.AddAppointmentBookingParent))]
        [OpenApiOperation(tags: _tag, Summary = "Add Appointment Booking Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddAppointmentBookingParentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddAppointmentBookingParent(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-parent")] HttpRequest req,
           [Queue("notification-app")] ICollector<string> collector,
           CancellationToken cancellationToken)
        {
            return _appointmentBookingParentHendler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.ApsentAppointmentBookingParent))]
        [OpenApiOperation(tags: _tag, Summary = "Apsent Appointment Booking Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ApsentAppointmentBookingParentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> ApsentAppointmentBookingParent(
          [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-parent")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _appointmentBookingParentHendler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.DetailAppointmentBookingParent))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Appointment Booking Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailAppointmentBookingParentResult))]
        public Task<IActionResult> DetailAppointmentBookingParent(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-parent/{id}")] HttpRequest req, string id,
           CancellationToken cancellationToken)
        {
            return _appointmentBookingParentHendler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetTeacherVenueMapping))]
        [OpenApiOperation(tags: _tag, Summary = "Get Teacher Venue Mapping")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetTeacherVenueMappingRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm[]))]
        public Task<IActionResult> GetTeacherVenueMapping(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-parent-teacher-venue-mapping")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetTeacherVenueMappingV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetAllChildDateilByParent))]
        [OpenApiOperation(tags: _tag, Summary = "Get All children Detail By Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAllChildDateilByParentRequest.IdParent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllChildDateilByParentRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllChildDateilByParentRequest.IsSiblingSameTime), In = ParameterLocation.Query, Type = typeof(bool), Required = true)]
        [OpenApiParameter(nameof(GetAllChildDateilByParentRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAllChildDateilByParentRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAllChildDateilByParentResult[]))]
        public Task<IActionResult> GetAllChildDateilByParent(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-parent-child-detail")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAllChildDateilByParentHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetStudentByAppointmentBooking))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student By Appointmant Booking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentByAppointmentBookingRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentByAppointmentBookingRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentByAppointmentBookingRequest.IdParent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentByAppointmentBookingRequest.IdUserTeacher), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetHomeroomStudent[]))]
        public Task<IActionResult> GetStudentByAppointmentBooking(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-parent-student")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentByAppointmentBookingHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetAppointmentBookingByUser))]
        [OpenApiOperation(tags: _tag, Summary = "Get Appointment Booing by user")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAppointmentBookingByUserRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAppointmentBookingByUserRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAppointmentBookingByUserRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAppointmentBookingByUserResult[]))]
        public Task<IActionResult> GetAppointmentBookingByUser(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-user")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAppointmentBookingByUserHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(AppointmentBookingEndpoint.GetAppointmentBookingDate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Date Appointment Booking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetAppointmentBookingDateRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm[]))]
        public Task<IActionResult> GetAppointmentBookingDate(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-date")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAppointmentBookingDateHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Available Time
        [FunctionName(nameof(AppointmentBookingEndpoint.GetAvailableTime))]
        [OpenApiOperation(tags: _tag, Summary = "Get Available Time")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAvailableTimeRequest.IdInvitationBookingSetting), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAvailableTimeRequest.IdUserTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAvailableTimeRequest.AppointmentDate), In = ParameterLocation.Query, Type = typeof(DateTime), Required = true)]
        [OpenApiParameter(nameof(GetAvailableTimeRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetAvailableTimeResult))]
        public Task<IActionResult> GetAvailableTime(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-available-time")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAvailableTimeHandler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region Reschedule Invitation Booking
        [FunctionName(nameof(AppointmentBookingEndpoint.UpdateInvitationBooking))]
        [OpenApiOperation(tags: _tag, Summary = "Reschedule and cancel invitation booking")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateInvitationBookingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdateInvitationBooking(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-reschedule-and-cancle")] HttpRequest req,
            [Queue("notification-app")] ICollector<string> collector,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UpdateInvitationBookingHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }
        #endregion
    }
}
