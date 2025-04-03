using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IClassDiary : IFnSchedule
    {
        #region class-diary
        [Get("/schedule/class-diary")]
        Task<ApiErrorResult<IEnumerable<GetClassDiaryResult>>> GetClassDiary(GetClassDiaryRequest query);

        [Post("/schedule/class-diary")]
        Task<ApiErrorResult> AddClassDiary([Body] AddClassDiaryRequest body);

        [Get("/schedule/class-diary/{id}")]
        Task<ApiErrorResult<GetDetailClassDiaryResult>> GetDetailClassDiary(string id);

        [Put("/schedule/class-diary")]
        Task<ApiErrorResult> UpdateClassDiary([Body] UpdateClassDiaryRequest body);

        [Delete("/schedule/class-diary")]
        Task<ApiErrorResult> DeleteClassDiary([Body] DeleteClassDiaryRequest body);

        //=========================

        [Get("/schedule/class-diary-subject")]
        Task<ApiErrorResult<IEnumerable<GetLevelClassDiaryResult>>> GetsubjectClassDiary(GetSubjectClassDiaryRequest query);

        [Get("/schedule/class-diary-homeroom-by-student")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetHomeroomByStudentClassDiary(GetHomeroomByStudentRequest query);

        [Post("/schedule/class-diary/class-id-by-student")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetClassIdByStudent([Body] GetClassIdByStudentRequest body);

        [Post("/schedule/class-diary/class-id-by-Teacher")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetClassIdByTeacher([Body] GetClassIdByStudentRequest body);

        [Get("/schedule/class-diary-start-end-date-period")]
        Task<ApiErrorResult<GetStartAndEndDatePeriodResult>> GetStartAndEndDatePeriod(GetStartAndEndDatePeriodRequest query);

        [Post("/schedule/class-diary/checking-type-setting")]
        Task<ApiErrorResult<IEnumerable<CheckTypeFeedbackResult>>> GetCheackingTypeSetting([Body] GetCheackingTypeSettingRequest body);

        [Post("/schedule/class-diary/type-setting")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetTypeSettingClassDiary([Body] GetTypeSettingClassDiaryRequest body);

        [Get("/schedule/class-diary-history")]
        Task<ApiErrorResult<IEnumerable<GetClassDiaryHistoryResult>>> GetClassDiaryHistory(GetClassDiaryHistoryRequest query);

        [Get("/schedule/class-diary-user-teacher-detail")]
        Task<ApiErrorResult<GetUserTeacherDetailByIdResult>> GetUserTeacherDetailById (GetUserTeacherDetailByIdRequest query);

        [Get("/schedule/class-diary-user-student-detail")]
        Task<ApiErrorResult<GetUserStudentDetailByIdResult>> GetUserStudentDetailById(GetUserTeacherDetailByIdRequest query);

        [Post("/schedule/class-diary/checking-class-limit")]
        Task<ApiErrorResult<IEnumerable<CheckClassLimitClassDiaryResult>>> GetCheackingClasslimit([Body] GetCheackingClassLimitRequest body);

        [Get("/schedule/class-diary-access")]
        Task<ApiErrorResult<GetCreateAccessClassDiaryResult>> GetCreateAccessClassDiary(GetCreateAccessClassDiaryRequest query);
        #endregion

        #region class-diary-deletion-approval
        [Get("/schedule/class-diary-deletion-approval")]
        Task<ApiErrorResult<IEnumerable<GetClassDiaryDeletionApprovalResult>>> GetClassDiaryDeletionApproval(GetClassDiaryDeletionApprovalRequest query);

        [Post("/schedule/class-diary-deletion-approval")]
        Task<ApiErrorResult> AddClassDiaryDeletionApproval([Body] AddClassDiaryDeletionApprovalRequest body);

        [Get("/schedule/class-diary-deletion-approval-Request-By")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetClassDiaryDeletionApprovalRequestBy(GetClassDiaryDeletionApprovalRequest query);

        [Get("/schedule/class-diary-deletion-approval/{id}")]
        Task<ApiErrorResult<GetDetailClassDiaryDeletionApprovalResult>> GetDetailClassDiaryDeletionApproval(string id);
        #endregion




    }
}
