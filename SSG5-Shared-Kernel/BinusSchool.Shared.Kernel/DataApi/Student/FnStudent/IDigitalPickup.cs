using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.DigitalPickup;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ScannerQRCode;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.Setting;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.QrCode;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IDigitalPickup : IFnStudent
    {
        #region ListPickedUp
        [Get("/student/digital-pickup/get-list-picked-up")]
        Task<ApiErrorResult<IEnumerable<GetListPickedUpResult>>> GetListPickedUp(GetListPickedUpRequest param);

        [Get("/student/digital-pickup/get-period-by-date")]
        Task<ApiErrorResult<GetPeriodByDateResult>> GetPeriodByDate(GetPeriodByDateRequest param);

        [Post("/student/digital-pickup/update-pickup-student")]
        Task<ApiErrorResult<IEnumerable<UpdatePickupStudentResult>>> UpdatePickupStudent([Body] UpdatePickupStudentRequest query);

        [Get("/student/digital-pickup/export-excel-list-pickup")]
        Task<HttpResponseMessage> ExportExcelListPickup(ExportExcelListPickupRequest body);
        #endregion

        #region ScannerQrCode
        [Post("/student/digital-pickup/add-student-pickup-using-qr")]
        Task<ApiErrorResult<AddStudentPickupUsingQRResult>> AddStudentPickupUsingQR([Body] AddStudentPickupUsingQRRequest query);
        #endregion

        #region Setting
        [Get("/student/digital-pickup/get-current-ay")]
        Task<ApiErrorResult<GetCurrentAYResult>> GetCurrentAY(GetCurrentAYRequest param);

        [Post("/student/digital-pickup/get-digital-pickup-setting")]
        Task<ApiErrorResult<IEnumerable<GetDigitalPickupSettingResult>>> GetDigitalPickupSetting([Body] GetDigitalPickupSettingRequest query);

        [Post("/student/digital-pickup/update-digital-pickup-setting")]
        Task<ApiErrorResult> UpdateDigitalPickupSetting([Body] UpdateDigitalPickupSettingRequest query);

        [Post("/student/digital-pickup/delete-digital-pickup-setting")]
        Task<ApiErrorResult> DeleteDigitalPickupSetting([Body] DeleteDigitalPickupSettingRequest query);
        #endregion

        #region QrCode
        [Get("/student/digital-pickup//get-student-for-digital-pickup")]
        Task<ApiErrorResult<IEnumerable<GetStudentForDigitalPickupResult>>> GetStudentForDigitalPickup(GetStudentForDigitalPickupRequest param);

        [Get("/student/digital-pickup/get-student-digital-pickup-qr")]
        Task<ApiErrorResult<StudentDigitalPickupQRResult>> GetStudentDigitalPickupQR(StudentDigitalPickupQRRequest param);

        [Post("/student/digital-pickup/add-student-digital-pickup-qr")]
        Task<ApiErrorResult<StudentDigitalPickupQRResult>> AddStudentDigitalPickupQR([Body] StudentDigitalPickupQRRequest query);

        [Get("/student/digital-pickup/send-email-notification-qr-code")]
        Task<ApiErrorResult> SendEmailNotificationQRCode(SendEmailNotificationQRCodeRequest param);
        #endregion

        #region Digital Pickup
        [Get("/student/digital-pickup/get-student-digital-pickup-history")]
        Task<ApiErrorResult<IEnumerable<GetStudentDigitalPickupHistoryResponse>>> GetStudentDigitalPickupHistory(GetStudentDigitalPickupHistoryRequest request);
        #endregion
    }
}
