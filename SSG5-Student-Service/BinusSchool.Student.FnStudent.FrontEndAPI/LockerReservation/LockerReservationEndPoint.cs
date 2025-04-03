using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.BookingPeriodSetting;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerAllocation;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.StudentBooking;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using BinusSchool.Student.FnStudent.LockerReservation.BookingPeriodSetting;
using BinusSchool.Student.FnStudent.LockerReservation.LockerAllocation;
using BinusSchool.Student.FnStudent.LockerReservation.LockerReservation;
using BinusSchool.Student.FnStudent.LockerReservation.StudentBooking;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.LockerReservation
{
    public class LockerReservationEndPoint
    {
        private const string _route = "locker-reservation";
        private const string _tag = "Locker Reservation";

        private readonly GetLockerBookingPeriodSettingHandler _getLockerBookingPeriodSettingHandler;
        private readonly GetLockerBookingPeriodGradeHandler _getLockerBookingPeriodGradeHandler;
        private readonly SaveLockerBookingPeriodGradeHandler _saveLockerBookingPeriodGradeHandler;
        private readonly GetLockerReservationPeriodPolicyHandler _getLockerReservationPeriodPolicyHandler;
        private readonly UpdateLockerReservationPeriodPolicyHandler _updateLockerReservationPeriodPolicyHandler;
        private readonly DeleteLockerReservationPeriodHandler _deleteLockerReservationPeriodHandler;

        private readonly GetAllLockerAllocationHandler _getAllLockerAllocationHandler;
        private readonly GetLockerAllocationBuildingHandler _getLockerAllocationBuildingHandler;
        private readonly GetLockerAllocationFloorHandler _getLockerAllocationFloorHandler;
        private readonly GetLockerAllocationGradeHandler _getLockerAllocationGradeHandler;
        private readonly SaveLockerAllocationHandler _saveLockerAllocationHandler;
        private readonly DeleteLockerAllocationHandler _deleteLockerAllocationHandler;
        private readonly CopyLockerAllocationFromLastSemesterHandler _copyLockerAllocationFromLastSemesterHandler;

        private readonly GetListLockerReservationHandler _getListLockerReservationHandler;
        private readonly CopyLockerReservationHandler _copyLockerReservationHandler;
        private readonly GetLockerListHandler _getLockerListHandler;
        private readonly DeleteLockerReservationHandler _deleteLockerReservationHandler;
        private readonly SaveLockedLockerHandler _saveLockedLockerHandler;
        private readonly UpdateLockedLockerHandler _updateLockedLockerHandler;
        private readonly SaveLockerReservationHandler _saveLockerReservationHandler;
        private readonly ExportExcelSummaryLockerReservationHandler _exportExcelSummaryLockerReservationHandler;

        private readonly GetStudentBookingHandler _getStudentBookingHandler;
        private readonly AddStudentReservationHandler _addStudentReservationHandler;

        public LockerReservationEndPoint(
            GetLockerBookingPeriodSettingHandler getLockerBookingPeriodSettingHandler,
            GetLockerBookingPeriodGradeHandler getLockerBookingPeriodGradeHandler,
            SaveLockerBookingPeriodGradeHandler saveLockerBookingPeriodGradeHandler,
            GetLockerReservationPeriodPolicyHandler getLockerReservationPeriodPolicyHandler,
            UpdateLockerReservationPeriodPolicyHandler updateLockerReservationPeriodPolicyHandler,
            DeleteLockerReservationPeriodHandler deleteLockerReservationPeriodHandler,

            GetAllLockerAllocationHandler getAllLockerAllocationHandler,
            GetLockerAllocationBuildingHandler getLockerAllocationBuildingHandler,
            GetLockerAllocationFloorHandler getLockerAllocationFloorHandler,
            GetLockerAllocationGradeHandler getLockerAllocationGradeHandler,
            SaveLockerAllocationHandler saveLockerAllocationHandler,
            DeleteLockerAllocationHandler deleteLockerAllocationHandler,
            CopyLockerAllocationFromLastSemesterHandler copyLockerAllocationFromLastSemesterHandler,

            GetListLockerReservationHandler getListLockerReservationHandler,
            CopyLockerReservationHandler copyLockerReservationHandler,
            GetLockerListHandler getLockerListHandler,
            DeleteLockerReservationHandler deleteLockerReservationHandler,
            SaveLockedLockerHandler saveLockedLockerHandler,
            UpdateLockedLockerHandler updateLockedLockerHandler,
            SaveLockerReservationHandler saveLockerReservationHandler,
            ExportExcelSummaryLockerReservationHandler exportExcelSummaryLockerReservationHandler,

            GetStudentBookingHandler getStudentBookingHandler,
            AddStudentReservationHandler addStudentReservationHandler)
        {
            _getLockerBookingPeriodSettingHandler = getLockerBookingPeriodSettingHandler;
            _getLockerBookingPeriodGradeHandler = getLockerBookingPeriodGradeHandler;
            _saveLockerBookingPeriodGradeHandler = saveLockerBookingPeriodGradeHandler;
            _getLockerReservationPeriodPolicyHandler = getLockerReservationPeriodPolicyHandler;
            _updateLockerReservationPeriodPolicyHandler = updateLockerReservationPeriodPolicyHandler;
            _deleteLockerReservationPeriodHandler = deleteLockerReservationPeriodHandler;

            _getAllLockerAllocationHandler = getAllLockerAllocationHandler;
            _getLockerAllocationBuildingHandler = getLockerAllocationBuildingHandler;
            _getLockerAllocationFloorHandler = getLockerAllocationFloorHandler;
            _getLockerAllocationGradeHandler = getLockerAllocationGradeHandler;
            _saveLockerAllocationHandler = saveLockerAllocationHandler;
            _deleteLockerAllocationHandler = deleteLockerAllocationHandler;
            _copyLockerAllocationFromLastSemesterHandler = copyLockerAllocationFromLastSemesterHandler;

            _getListLockerReservationHandler = getListLockerReservationHandler;
            _copyLockerReservationHandler = copyLockerReservationHandler;
            _getLockerListHandler = getLockerListHandler;
            _deleteLockerReservationHandler = deleteLockerReservationHandler;
            _saveLockedLockerHandler = saveLockedLockerHandler;
            _updateLockedLockerHandler = updateLockedLockerHandler;
            _saveLockerReservationHandler = saveLockerReservationHandler;
            _exportExcelSummaryLockerReservationHandler = exportExcelSummaryLockerReservationHandler;

            _getStudentBookingHandler = getStudentBookingHandler;
            _addStudentReservationHandler = addStudentReservationHandler;
        }

        #region Booking Period Setting
        [FunctionName(nameof(LockerReservationEndPoint.GetLockerBookingPeriodSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Locker Booking Period Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLockerBookingPeriodSettingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetLockerBookingPeriodSettingResult>))]
        public Task<IActionResult> GetLockerBookingPeriodSetting(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-locker-booking-period-setting")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getLockerBookingPeriodSettingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.GetLockerBookingPeriodGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Locker Booking Period Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLockerBookingPeriodGradeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLockerBookingPeriodGradeRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetLockerBookingPeriodGradeResult>))]
        public Task<IActionResult> GetLockerBookingPeriodGrade(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-locker-booking-period-grade")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getLockerBookingPeriodGradeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.SaveLockerBookingPeriodGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Save Locker Booking Period Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveLockerBookingPeriodGradeRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveLockerBookingPeriodGrade(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-locker-booking-period-grade")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveLockerBookingPeriodGradeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.GetLockerReservationPeriodPolicy))]
        [OpenApiOperation(tags: _tag, Summary = "Get Locker Reservation Period Policy")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLockerReservationPeriodPolicyRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLockerReservationPeriodPolicyRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetLockerReservationPeriodPolicyResult>))]
        public Task<IActionResult> GetLockerReservationPeriodPolicy(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-locker-reservation-period-policy")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getLockerReservationPeriodPolicyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.UpdateLockerReservationPeriodPolicy))]
        [OpenApiOperation(tags: _tag, Summary = "Update Locker Reservation Period Policy")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<UpdateLockerReservationPeriodPolicyRequest>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateLockerReservationPeriodPolicy(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/update-locker-reservation-period-policy")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateLockerReservationPeriodPolicyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.DeleteLockerReservationPeriod))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Locker Reservation Period")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteLockerReservationPeriodRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.NoContent)]
        public Task<IActionResult> DeleteLockerReservationPeriod(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-locker-reservation-period")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteLockerReservationPeriodHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Locker Allocation
        [FunctionName(nameof(LockerReservationEndPoint.GetAllLockerAllocation))]
        [OpenApiOperation(tags: _tag, Summary = "Get All Locker Allocation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAllLockerAllocationRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAllLockerAllocationRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetAllLockerAllocationResult>))]
        public Task<IActionResult> GetAllLockerAllocation(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-all-locker-allocation")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getAllLockerAllocationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.GetLockerAllocationBuilding))]
        [OpenApiOperation(tags: _tag, Summary = "Get Locker Allocation Building")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLockerAllocationBuildingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetLockerAllocationBuildingResult>))]
        public Task<IActionResult> GetLockerAllocationBuilding(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-locker-allocation-building")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getLockerAllocationBuildingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.GetLockerAllocationFloor))]
        [OpenApiOperation(tags: _tag, Summary = "Get Locker Allocation Floor")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLockerAllocationFloorRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLockerAllocationFloorRequest.IdBuilding), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetLockerAllocationFloorResult>))]
        public Task<IActionResult> GetLockerAllocationFloor(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-locker-allocation-floor")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getLockerAllocationFloorHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.GetLockerAllocationGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Locker Allocation Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetLockerAllocationGradeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLockerAllocationGradeRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetLockerAllocationGradeRequest.IdBuilding), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetLockerAllocationGradeRequest.IdFloor), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetLockerAllocationGradeResult>))]
        public Task<IActionResult> GetLockerAllocationGrade(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-locker-allocation-grade")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getLockerAllocationGradeHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.SaveLockerAllocation))]
        [OpenApiOperation(tags: _tag, Summary = "Save Locker Allocation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveLockerAllocationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveLockerAllocation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-locker-allocation")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveLockerAllocationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.DeleteLockerAllocation))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Locker Allocation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteLockerAllocationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteLockerAllocation(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-locker-allocation")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteLockerAllocationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.CopyLockerAllocationFromLastSemester))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Locker Allocation From Last Semester")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopyLockerAllocationFromLastSemesterRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopyLockerAllocationFromLastSemester(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/copy-locker-allocation-from-last-semester")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _copyLockerAllocationFromLastSemesterHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Locker Reservation
        [FunctionName(nameof(LockerReservationEndPoint.GetListLockerReservation))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Locker Reservation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetListLockerReservationRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListLockerReservationResult))]
        public Task<IActionResult> GetListLockerReservation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-list-locker-reservation")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getListLockerReservationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.CopyLockerReservation))]
        [OpenApiOperation(tags: _tag, Summary = "Copy Locker Reservation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CopyLockerReservationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> CopyLockerReservation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/copy-locker-reservation")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _copyLockerReservationHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(LockerReservationEndPoint.GetLockerList))]
        [OpenApiOperation(tags: _tag, Summary = "Get Locker List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetLockerListRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetLockerListResult))]
        public Task<IActionResult> GetLockerList(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-locker-list")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getLockerListHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.DeleteLockerReservation))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Locker Reservation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteLockerReservationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteLockerReservation(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete-locker-reservation")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteLockerReservationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.SaveLockedLocker))]
        [OpenApiOperation(tags: _tag, Summary = "Save Locked Locker")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveLockedLockerRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveLockedLocker(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-locked-locker")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveLockedLockerHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.UpdateLockedLocker))]
        [OpenApiOperation(tags: _tag, Summary = "Update Locked Locker")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateLockedLockerRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateLockedLocker(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/update-locked-locker")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateLockedLockerHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.SaveLockerReservation))]
        [OpenApiOperation(tags: _tag, Summary = "Save Locker Reservation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SaveLockerReservationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SaveLockerReservation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/save-locker-reservation")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _saveLockerReservationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.ExportExcelSummaryLockerReservation))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel Summary Locker Reservation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ExportExcelSummaryLockerReservationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelSummaryLockerReservation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/export-excel-summary-locker-reservation")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _exportExcelSummaryLockerReservationHandler.Execute(req, cancellationToken, false);
        }
        #endregion

        #region Student Booking 

        [FunctionName(nameof(LockerReservationEndPoint.GetStudentBooking))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Booking Locker")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentBookingRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentBookingRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentBookingResult))]
        public Task<IActionResult> GetStudentBooking(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-booking-locker")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentBookingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(LockerReservationEndPoint.AddStudentReservation))]
        [OpenApiOperation(tags: _tag, Summary = "Add Student Reservation Locker")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddStudentReservationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddStudentReservation(
         [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-student-reservation-locker")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _addStudentReservationHandler.Execute(req, cancellationToken);
        }
        #endregion
    }
}
