using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.CurriculumSettings;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.SubjectSelection.SubjectGroupSettings;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataApi.Student.FnStudent
{
    public interface ISubjectSelection : IFnStudent
    {
        #region Curriculum Settings
        [Get("/student/curriculum-settings/get-subject-selection-curriculum")]
        Task<ApiErrorResult<IEnumerable<GetSubjectSelectionCurriculumResult>>> GetSubjectSelectionCurriculum(GetSubjectSelectionCurriculumRequest param);

        [Post("/student/curriculum-settings/save-subject-selection-curriculum")]
        Task<ApiErrorResult> SaveSubjectSelectionCurriculum([Body] SaveSubjectSelectionCurriculumRequest query);

        [Delete("/student/curriculum-settings/delete-subject-selection-curriculum")]
        Task<ApiErrorResult> DeleteSubjectSelectionCurriculum([Body] DeleteSubjectSelectionCurriculumRequest query);

        [Get("/student/curriculum-settings/get-list-subject-selection-curriculum-mapping")]
        Task<ApiErrorResult<IEnumerable<GetListSubjectSelectionCurriculumMappingResult>>> GetListSubjectSelectionCurriculumMapping(GetListSubjectSelectionCurriculumMappingRequest param);

        [Get("/student/curriculum-settings/get-detail-subject-selection-curriculum-mapping")]
        Task<ApiErrorResult<GetDetailSubjectSelectionCurriculumMappingResult>> GetDetailSubjectSelectionCurriculumMapping(GetDetailSubjectSelectionCurriculumMappingRequest param);

        [Post("/student/curriculum-settings/save-subject-selection-curriculum-mapping")]
        Task<ApiErrorResult> SaveSubjectSelectionCurriculumMapping([Body] SaveSubjectSelectionCurriculumMappingRequest query);

        [Delete("/student/curriculum-settings/delete-subject-selection-curriculum-mapping")]
        Task<ApiErrorResult> DeleteSubjectSelectionCurriculumMapping([Body] DeleteSubjectSelectionCurriculumMappingRequest query);

        [Post("/student/curriculum-settings/copy-subject-selection-curriculum-mapping")]
        Task<ApiErrorResult> CopySubjectSelectionCurriculumMapping([Body] CopySubjectSelectionCurriculumMappingRequest query);
        #endregion

        #region Subject Group Settings
        [Get("/student/subject-selection/subject-group-settings/curriculum")]
        Task<ApiErrorResult<IEnumerable<GetSubjectGroupSettingsCurriculumResponse>>> GetSubjectGroupSettingsCurriculum(GetSubjectGroupSettingsCurriculumRequest request);

        [Get("/student/subject-selection/subject-group-settings/curriculum-mapping")]
        Task<ApiErrorResult<IEnumerable<GetSubjectGroupSettingsCurriculumMappingResponse>>> GetSubjectGroupSettingsCurriculumMapping(GetSubjectGroupSettingsCurriculumMappingRequest request);

        [Put("/student/subject-selection/subject-group-settings/change-curriculum-mapping-status")]
        Task<ApiErrorResult> ChangeSubjectGroupSettingsCurriculumMappingStatus([Body] ChangeSubjectGroupSettingsCurriculumMappingStatusRequest query);

        [Delete("/student/subject-selection/subject-group-settings/delete-curriculum-mapping")]
        Task<ApiErrorResult> DeleteSubjectGroupSettingsCurriculumMapping([Body] DeleteSubjectGroupSettingsCurriculumMappingRequest query);

        [Get("/student/subject-selection/subject-group-settings/subject-selection-group")]
        Task<ApiErrorResult<IEnumerable<GetSubjectGroupSettingsSubjectSelectionGroupResponse>>> GetSubjectGroupSettingsSubjectSelectionGroup(GetSubjectGroupSettingsSubjectSelectionGroupRequest request);

        [Post("/student/subject-selection/subject-group-settings/save-subject-selection-group")]
        Task<ApiErrorResult> SaveSubjectGroupSettingsSubjectSelectionGroup([Body] SaveSubjectGroupSettingsSubjectSelectionGroupRequest request);

        [Put("/student/subject-selection/subject-group-settings/change-subject-selection-group-status")]
        Task<ApiErrorResult> ChangeSubjectGroupSettingsSubjectSelectionGroupStatus([Body] ChangeSubjectGroupSettingsSubjectSelectionGroupStatusRequest request);

        [Delete("/student/subject-selection/subject-group-settings/delete-subject-selection-group")]
        Task<ApiErrorResult> DeleteSubjectGroupSettingsSubjectSelectionGroup([Body] DeleteSubjectGroupSettingsSubjectSelectionGroupRequest request);
        #endregion
    }
}
