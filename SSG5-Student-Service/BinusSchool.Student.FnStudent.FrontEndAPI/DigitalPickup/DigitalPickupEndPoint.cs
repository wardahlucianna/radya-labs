using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using BinusSchool.Student.FnStudent.DigitalPickup.ListPickedUp;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp;
using BinusSchool.Student.FnStudent.DigitalPickup.ScannerQRCode;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ScannerQRCode;
using BinusSchool.Student.FnStudent.DigitalPickup.Setting;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.Setting;
using BinusSchool.Student.FnStudent.DigitalPickup.DigitalPickup;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.DigitalPickup;
using BinusSchool.Student.FnStudent.DigitalPickup.QrCode;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.QrCode;

namespace BinusSchool.Student.FnStudent.District
{
    public class DigitalPickupEndPoint
    {
        private const string _route = "student/digital-pickup";
        private const string _tag = "Digital Pickup";

        private readonly GetPeriodByDateHandler _getPeriodByDateHandler;
        private readonly GetListPickedUpHandler _getListPickedUpHandler;
        private readonly AddStudentPickupUsingQRHandler _addStudentPickupUsingQRHandler;
        private readonly GetCurrentAYHandler _getCurrentAYHandler;
        private readonly GetDigitalPickupSettingHandler _getDigitalPickupSettingHandler;
        private readonly UpdatePickupStudentHandler _updatePickupStudentHandler;
        private readonly UpdateDigitalPickupSettingHandler _updateDigitalPickupSettingHandler;
        private readonly DeleteDigitalPickupSettingHandler _deleteDigitalPickupSettingHandler;
        private readonly ExportExcelListPickupHandler _exportExcelListPickupHandler;
        private readonly GetStudentForDigitalPickupHandler _getStudentForDigitalPickupHandler;
        private readonly GetStudentDigitalPickupQRHandler _getStudentDigitalPickupQRHandler;
        private readonly GetStudentDigitalPickupHistoryHandler _getStudentDigitalPickupHistoryHandler;
        private readonly AddStudentDigitalPickupQRHandler _addStudentDigitalPickupQRHandler;
        private readonly SendEmailNotificationQRCodeHandler _sendEmailNotificationQRCodeHandler;
        public DigitalPickupEndPoint(
            GetPeriodByDateHandler getPeriodByDateHandler,
            GetListPickedUpHandler getListPickedUpHandler,
            AddStudentPickupUsingQRHandler addStudentPickupUsingQRHandler,
            GetCurrentAYHandler getCurrentAYHandler,
            GetDigitalPickupSettingHandler getDigitalPickupSettingHandler,
            UpdatePickupStudentHandler updatePickupStudentHandler,
            UpdateDigitalPickupSettingHandler updateDigitalPickupSettingHandler,
            DeleteDigitalPickupSettingHandler deleteDigitalPickupSettingHandler,
            ExportExcelListPickupHandler exportExcelListPickupHandler,
            GetStudentForDigitalPickupHandler getStudentForDigitalPickupHandler,
            GetStudentDigitalPickupQRHandler getStudentDigitalPickupQRHandler,
            GetStudentDigitalPickupHistoryHandler getStudentDigitalPickupHistoryHandler,
            AddStudentDigitalPickupQRHandler addStudentDigitalPickupQRHandler,
            SendEmailNotificationQRCodeHandler sendEmailNotificationQRCodeHandler
            )
        {
            _getPeriodByDateHandler = getPeriodByDateHandler;
            _getListPickedUpHandler = getListPickedUpHandler;
            _addStudentPickupUsingQRHandler = addStudentPickupUsingQRHandler;
            _getCurrentAYHandler = getCurrentAYHandler;
            _getDigitalPickupSettingHandler = getDigitalPickupSettingHandler;
            _updatePickupStudentHandler = updatePickupStudentHandler;
            _updateDigitalPickupSettingHandler = updateDigitalPickupSettingHandler;
            _deleteDigitalPickupSettingHandler = deleteDigitalPickupSettingHandler;
            _exportExcelListPickupHandler = exportExcelListPickupHandler;
            _getStudentForDigitalPickupHandler = getStudentForDigitalPickupHandler;
            _getStudentDigitalPickupQRHandler = getStudentDigitalPickupQRHandler;
            _getStudentDigitalPickupHistoryHandler = getStudentDigitalPickupHistoryHandler;
            _addStudentDigitalPickupQRHandler = addStudentDigitalPickupQRHandler;
            _sendEmailNotificationQRCodeHandler = sendEmailNotificationQRCodeHandler;
        }

        #region ListPickedUp
        [FunctionName(nameof(DigitalPickupEndPoint.GetPeriodByDate))]
        [OpenApiOperation(tags: _tag, Summary = "Get Period By Date")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetPeriodByDateRequest.Date), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPeriodByDateRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPeriodByDateResult))]
        public Task<IActionResult> GetPeriodByDate(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-period-by-date")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getPeriodByDateHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DigitalPickupEndPoint.GetListPickedUp))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Picked Up")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListPickedUpRequest.Date), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetListPickedUpRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListPickedUpRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListPickedUpRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetListPickedUpRequest.Status), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetListPickedUpResult))]
        public Task<IActionResult> GetListPickedUp(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-picked-up")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getListPickedUpHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DigitalPickupEndPoint.UpdatePickupStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Update Pickup Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdatePickupStudentRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(UpdatePickupStudentResult))]
        public Task<IActionResult> UpdatePickupStudent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/update-pickup-student")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updatePickupStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DigitalPickupEndPoint.ExportExcelListPickup))]
        [OpenApiOperation(tags: _tag, Summary = "Export Excel List Pickup")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(ExportExcelListPickupRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ExportExcelListPickupRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ExportExcelListPickupRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(ExportExcelListPickupRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(ExportExcelListPickupRequest.IdHomeroom), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(ExportExcelListPickupRequest.StartDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(ExportExcelListPickupRequest.EndDate), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ExportExcelListPickup(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/export-excel-list-pickup")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _exportExcelListPickupHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region ScannerQrCode
        [FunctionName(nameof(DigitalPickupEndPoint.AddStudentPickupUsingQR))]
        [OpenApiOperation(tags: _tag, Summary = "Add Student Pickup Using QR")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddStudentPickupUsingQRRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AddStudentPickupUsingQRResult))]
        public Task<IActionResult> AddStudentPickupUsingQR(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-student-pickup-using-qr")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _addStudentPickupUsingQRHandler.Execute(req, cancellationToken, false);
        }
        #endregion

        #region Setting
        [FunctionName(nameof(DigitalPickupEndPoint.GetCurrentAY))]
        [OpenApiOperation(tags: _tag, Summary = "Get Current AY")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetCurrentAYRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetCurrentAYResult))]
        public Task<IActionResult> GetCurrentAY(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-current-ay")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getCurrentAYHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DigitalPickupEndPoint.GetDigitalPickupSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Get Digital Pickup Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetDigitalPickupSettingRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetDigitalPickupSettingResult))]
        public Task<IActionResult> GetDigitalPickupSetting(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-digital-pickup-setting")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getDigitalPickupSettingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DigitalPickupEndPoint.UpdateDigitalPickupSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Update Digital Pickup Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateDigitalPickupSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateDigitalPickupSetting(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/update-digital-pickup-setting")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _updateDigitalPickupSettingHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DigitalPickupEndPoint.DeleteDigitalPickupSetting))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Digital Pickup Setting")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteDigitalPickupSettingRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteDigitalPickupSetting(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/delete-digital-pickup-setting")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _deleteDigitalPickupSettingHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region QRCode
        [FunctionName(nameof(DigitalPickupEndPoint.GetStudentForDigitalPickup))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student For Digital Pickup")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentForDigitalPickupRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentForDigitalPickupRequest.IdParent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentForDigitalPickupResult))]
        public Task<IActionResult> GetStudentForDigitalPickup(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-for-digital-pickup")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getStudentForDigitalPickupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DigitalPickupEndPoint.GetStudentDigitalPickupQR))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Digital Pickup QR")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(StudentDigitalPickupQRRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(StudentDigitalPickupQRRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(StudentDigitalPickupQRRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(StudentDigitalPickupQRResult))]
        public Task<IActionResult> GetStudentDigitalPickupQR(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-digital-pickup-qr")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getStudentDigitalPickupQRHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DigitalPickupEndPoint.AddStudentDigitalPickupQR))]
        [OpenApiOperation(tags: _tag, Summary = "Add Student Digital Pickup QR")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(StudentDigitalPickupQRRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(StudentDigitalPickupQRResult))]
        public Task<IActionResult> AddStudentDigitalPickupQR(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = _route + "/add-student-digital-pickup-qr")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _addStudentDigitalPickupQRHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(DigitalPickupEndPoint.SendEmailNotificationQRCode))]
        [OpenApiOperation(tags: _tag, Summary = "Send Email Notification QR Code")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(SendEmailNotificationQRCodeRequest.IdDigitalPickupQrCode), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SendEmailNotificationQRCode(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/send-email-notification-qr-code")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _sendEmailNotificationQRCodeHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Digital Pickup
        [FunctionName(nameof(DigitalPickupEndPoint.GetStudentDigitalPickupHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Digital Pickup History")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentDigitalPickupHistoryRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentDigitalPickupHistoryRequest.Semester), In = ParameterLocation.Query, Type = typeof(int), Required = true)]
        [OpenApiParameter(nameof(GetStudentDigitalPickupHistoryRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentDigitalPickupHistoryResponse>))]
        public Task<IActionResult> GetStudentDigitalPickupHistory(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-digital-pickup-history")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getStudentDigitalPickupHistoryHandler.Execute(req, cancellationToken);
        }
        #endregion
    }
}
