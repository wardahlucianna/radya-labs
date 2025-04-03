using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiaryTypeSetting;
using Refit;
namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IClassDiaryTypeSetting : IFnSchedule
    {
        [Get("/schedule/class-diary-type-setting")]
        Task<ApiErrorResult<IEnumerable<GetClassDiaryTypeSettingResult>>> GetListClassDiaryTypeSetting(GetClassDiaryTypeSettingRequest query);

        [Get("/schedule/class-diary-type-setting/list-lesson-exclude")]
        Task<ApiErrorResult<IEnumerable<GetClassDiaryLessonExcludeResult>>> GetListClassDiaryLessonExclude(GetClassDiaryLessonExcludeRequest query);

        [Get("/schedule/class-diary-type-setting/list-grade")]
        Task<ApiErrorResult<IEnumerable<GetGradeClassDiaryTypeSettingResult>>> GetGradeClassDiaryLessonExclude(GetGradeClassDiaryTypeSettingRequest query);

        [Get("/schedule/class-diary-type-setting/list-subject")]
        Task<ApiErrorResult<IEnumerable<GetSubjectClassDiaryTypeSettingResult>>> GetSubjectClassDiaryLessonExclude(GetSubjectClassDiaryTypeSettingRequest query);

        [Get("/schedule/class-diary-type-setting/detail/{id}")]
        Task<ApiErrorResult<GetClassDiaryTypeSettingDetailResult>> GetClassDiaryTypeSettingDetail(string id);

        [Post("/schedule/class-diary-type-setting")]
        Task<ApiErrorResult> AddClassDiaryTypeSetting([Body] AddClassDiaryTypeSettingRequest body);

        [Put("/schedule/class-diary-type-setting")]
        Task<ApiErrorResult> UpdateClassDiaryTypeSetting([Body] UpdateClassDiaryTypeSettingRequest body);

        [Delete("/schedule/class-diary-type-setting")]
        Task<ApiErrorResult> DeleteClassDiaryTypeSetting([Body] IEnumerable<string> ids);

        [Post("/schedule/class-diary-type-setting/copy-setting")]
        Task<ApiErrorResult> CopySettingClassDiaryTypeSetting([Body] CopySettingClassDiaryTypeSettingRequest body);

        [Post("/schedule/class-diary-type-setting/get-same-data-copy-setting")]
        Task<ApiErrorResult> GetSameDataCopySettingClassDiaryTypeSetting([Body] CopySettingClassDiaryTypeSettingRequest body);
    }
}
