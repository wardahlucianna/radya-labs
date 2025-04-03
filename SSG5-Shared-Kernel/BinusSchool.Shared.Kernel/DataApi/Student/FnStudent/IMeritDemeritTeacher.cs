using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemerit;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IMeritDemeritTeacher : IFnStudent
    {
        #region Freeze
        [Get("/student/merit-demerit-teacher/freeze")]
        Task<ApiErrorResult<IEnumerable<GetFreezeResult>>> GetFreeze(GetFreezeRequest query);

        [Post("/student/merit-demerit-teacher/freeze")]
        Task<ApiErrorResult> UpdateFreeze([Body] UpdateFreezeRequest body);
        #endregion 

        #region Entry Merit Demerit
        [Get("/student/merit-demerit-teacher/disipline")]
        Task<ApiErrorResult<IEnumerable<GetEntryMeritDemeritDisiplineResult>>> GetEntryMeritDemeritDisipline(GetEntryMeritDemeritDisiplineRequest query);

        [Get("/student/merit-demerit-teacher/student-by-freeze")]
        Task<ApiErrorResult<IEnumerable<GetEntryMeritDemeritStudentByFreezeResult>>> GetEntryMeritDemeritStudentByFreeze(GetEntryMeritDemeritStudentByFreezeRequest query);

        [Get("/student/merit-demerit-teacher/entry")]
        Task<ApiErrorResult<IEnumerable<GetMeritDemeritTeacherResult>>> GetMeritDemeritTeacher(GetMeritDemeritTeacherRequest query);

        [Post("/student/merit-demerit-teacher/entry")]
        Task<ApiErrorResult<IEnumerable<AddMeritDemeritTeacherResult>>> AddMeritDemeritTeacher([Body] AddMeritDemeritTeacherRequest query);

        [Get("/student/merit-demerit-teacher/entry-merit-detail")]
        Task<ApiErrorResult<GetDetailMeritTeacherResult>> GetDetailMeritTeacher(GetDetailMeritTeacherRequest query);

        [Get("/student/merit-demerit-teacher/entry-merit-detail-v2")]
        Task<ApiErrorResult<GetDetailMeritTeacherV2Result>> GetDetailMeritTeacherV2(GetDetailMeritTeacherV2Request query);

        [Get("/student/merit-demerit-teacher/entry-demerit-detail")]
        Task<ApiErrorResult<GetDetailDemeritTeacherResult>> GetDetailDemeritTeacher(GetDetailMeritTeacherRequest query);

        [Delete("/student/merit-demerit-teacher/entry-merit")]
        Task<ApiErrorResult> DeleteEntryMeritTeacher([Body] DeleteEntryMeritDemeritTeacherRequest query);

        [Delete("/student/merit-demerit-teacher/entry-demerit")]
        Task<ApiErrorResult> DeleteEntryDemeritTeacher([Body] DeleteEntryMeritDemeritTeacherRequest query);

        [Get("/student/merit-demerit-teacher/entry-exsis")]
        Task<ApiErrorResult<IEnumerable<GetExsisEntryMeritDemeritTeacherByIdResult>>> GetExsisEntryMeritDemeritTeacherById([Body] GetExsisEntryMeritDemeritTeacherByIdRequest query);

        [Post("/student/merit-demerit-teacher/download-excel")]
        Task<HttpResponseMessage> GetDownloadTeacherMeritDemeritTeacher([Body] GetDownloadTeacherMeritDemeritTeacherRequest body);

        [Post("/student/merit-demerit-teacher/download-merit-achievement")]
        Task<HttpResponseMessage> DownloadMeritAndAchievement([Body] GetDetailMeritTeacherV2Request body);

        [Post("/student/merit-demerit-teacher/entry-detail")]
        Task<ApiErrorResult<IEnumerable<GetDetailEntryMeritDemeritResult>>> GetDetailEntryMeritDemerit([Body] GetDetailMeritTeacherV2Request query);
        #endregion

        #region approval
        [Get("/student/merit-demerit-teacher/approval")]
        Task<ApiErrorResult<IEnumerable<GetApprovalMeritDemeritTeacherResult>>> GetApprovalMeritDemeritTeacher(GetApprovalMeritDemeritTeacherRequest query);

        [Post("/student/merit-demerit-teacher/approval")]
        Task<ApiErrorResult> ApprovalMeritDemeritTeacher([Body] ApprovalMeritDemeritTeacherRequest query);

        [Put("/student/merit-demerit-teacher/approval")]
        Task<ApiErrorResult> DeclineMeritDemeritTeacher([Body] ApprovalMeritDemeritTeacherRequest query);

        [Post("/student/merit-demerit-teacher/approval-detail")]
        Task<ApiErrorResult<GetDetailApprovalMeritDemeritResult>> GetDetailApprovalMeritDemerit([Body] GetDetailApprovalMeritDemeritRequest query);
        #endregion

        #region History approval

        [Get("/student/merit-demerit-teacher/history-approval")]
        Task<ApiErrorResult<IEnumerable<GetHistoryApprovalMeritDemeritTeacherResult>>> GetHistoryApprovalMeritDemeritTeacher(GetHistoryApprovalMeritDemeritTeacherRequest query);

        #endregion

        #region   Wizard
        [Get("/student/merit-demerit-teacher/wizard-student")]
        Task<ApiErrorResult<WizardStudentHandlerResult>> GetWizardStudent(WizardStudentHandlerRequest query);

        [Get("/student/merit-demerit-teacher/wizard-student-detail-merit")]
        Task<ApiErrorResult<WizardStudentDetailMeritResult>> GetWizardStudentDetailMerit(WizardDetailStudentRequest query);

        [Get("/student/merit-demerit-teacher/wizard-student-detail-merit-v2")]
        Task<ApiErrorResult<WizardStudentDetailMeritV2Result>> GetWizardStudentDetailMeritV2(WizardDetailStudentRequest query);

        [Get("/student/merit-demerit-teacher/wizard-student-detail-demerit")]
        Task<ApiErrorResult<WizardStudentDetailDemeritResult>> GetWizardStudentDetailDemerit(WizardDetailStudentRequest query);

        [Get("/student/merit-demerit-teacher/wizard-teacher")]
        Task<ApiErrorResult<IEnumerable<WizardTeacherResult>>> GetWizardTeacher(WizardStudentHandlerRequest query);

        [Get("/student/merit-demerit-teacher/wizard-student-Achievement")]
        Task<ApiErrorResult<IEnumerable<GetWizardStudentAchievementResult>>> GetWizardStudentAchievement(GetWizardStudentAchievementRequest query);

        [Get("/student/merit-demerit-teacher/point-system-by-user")]
        Task<ApiErrorResult<bool>> GetPointSystemByIdUser(GetPointSystemByIdUserRequest query);
        #endregion

        #region Entry Merit Student
        [Post("/student/merit-demerit-teacher/merit-student")]
        Task<ApiErrorResult<IEnumerable<AddMeritDemeritTeacherResult>>> AddMeritStudent([Body] AddMeritStudentRequest query);
        #endregion

        #region CornJob Point
        [Post("/student/merit-demerit-teacher/reset-point")]
        Task<ApiErrorResult> ResetMeritDemeritPoint([Body] ResetMeritDemeritPointRequest query);

        [Post("/student/merit-demerit-teacher/calculate-point")]
        Task<ApiErrorResult> CalculateMeritDemeritPoint([Body] CalculateMeritDemeritPointRequest body);

        [Post("/student/merit-demerit-teacher/calculate-point-smt1")]
        Task<ApiErrorResult> CalculateMeritDemeritPointSmt1(CalculateMeritDemeritPointRequest param);

        [Post("/student/merit-demerit-teacher/reset-point-v2")]
        Task<ApiErrorResult> ResetMeritDemeritPointV2([Body] ResetMeritDemeritPointV2Request query);
    #endregion

    #region Update Merit Demerit
    [Get("/student/merit-demerit-teacher/entry-merit-detail-by-id")]
        Task<ApiErrorResult<GetDetailEntryMeritDemeritByIdResult>> GetDetailEntryMeritDemeritById(GetDetailEntryMeritDemeritByIdRequest query);

        [Put("/student/merit-demerit-teacher/entry-merit-by-id")]
        Task<ApiErrorResult> UpdateMeritDemeritById([Body] UpdateMeritDemeritByIdRequest query);
        #endregion
    }
}
