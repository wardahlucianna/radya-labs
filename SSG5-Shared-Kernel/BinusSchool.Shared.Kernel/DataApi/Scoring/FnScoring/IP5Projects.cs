using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5Entry;
using BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5ProjectSettings;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ViewTemplate;
using BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IP5Projects : IFnScoring
    {
        #region P5 Project Settings
        [Get("/p5-projects/get-p5-project-setting-list")]
        Task<ApiErrorResult<IEnumerable<GetP5ProjectSettingListResult>>> GetP5ProjectSettingList(GetP5ProjectSettingListRequest query);

        [Get("/p5-projects/get-p5-project-setting-theme")]
        Task<ApiErrorResult<IEnumerable<GetP5ProjectSettingThemeResult>>> GetP5ProjectSettingTheme(GetP5ProjectSettingThemeRequest query);

        [Get("/p5-projects/get-p5-project-setting-detail")]
        Task<ApiErrorResult<GetP5ProjectSettingDetailResult>> GetP5ProjectSettingDetail(GetP5ProjectSettingDetailRequest query);

        [Post("/p5-projects/get-p5-project-setting-project-outcomes")]
        Task<ApiErrorResult<IEnumerable<GetP5ProjectSettingProjectOutcomesResult>>> GetP5ProjectSettingProjectOutcomes([Body] GetP5ProjectSettingProjectOutcomesRequest param);

        [Get("/p5-projects/get-p5-project-setting-dimension")]
        Task<ApiErrorResult<IEnumerable<GetP5ProjectSettingDimensionResult>>> GetP5ProjectSettingDimension(GetP5ProjectSettingDimensionRequest query);

        [Get("/p5-projects/get-p5-project-setting-element")]
        Task<ApiErrorResult<IEnumerable<GetP5ProjectSettingElementResult>>> GetP5ProjectSettingElement(GetP5ProjectSettingElementRequest query);

        [Get("/p5-projects/get-p5-project-setting-sub-element")]
        Task<ApiErrorResult<IEnumerable<GetP5ProjectSettingSubElementResult>>> GetP5ProjectSettingSubElement(GetP5ProjectSettingSubElementRequest query);

        [Post("/p5-projects/create-p5-project-setting-project-outcomes")]
        Task<ApiErrorResult<IEnumerable<CreateP5ProjectSettingProjectOutcomesResult>>> CreateP5ProjectSettingProjectOutcomes([Body] CreateP5ProjectSettingProjectOutcomesRequest param);

        [Post("/p5-projects/save-p5-project-setting")]
        Task<ApiErrorResult> SaveP5ProjectSetting([Body] SaveP5ProjectSettingRequest param);

        [Get("/p5-projects/get-p5-project-setting-kurnas-phase-grade")]
        Task<ApiErrorResult<IEnumerable<GetP5ProjectSettingKurnasPhaseGradeResult>>> GetP5ProjectSettingKurnasPhaseGrade(GetP5ProjectSettingKurnasPhaseGradeRequest query);

        [Delete("/p5-projects/delete-p5-project-setting")]
        Task<ApiErrorResult> DeleteP5ProjectSetting([Body] DeleteP5ProjectSettingRequest param);
        #endregion

        #region P5 Entry 
        [Get("/p5-projects/get-p5-list")]
        Task<ApiErrorResult<IEnumerable<GetP5EntryListResult>>> GetP5EntryList(GetP5EntryListRequest query);
        
        [Get("/p5-projects/get-p5-entry-privilege-list")]
        Task<ApiErrorResult<GetP5EntryPrivilegeResult>> GetP5EntryPrivilegeList(GetP5EntryPrivilegeRequest query);

        [Get("/p5-projects/get-student-p5-entry-list")]
        Task<ApiErrorResult<GetStudentP5EntryResult>> GetStudentP5Entry(GetStudentP5EntryRequest query);

        [Delete("/p5-projects/delete-student-p5-entry")]
        Task<ApiErrorResult> DeleteStudentP5Entry([Body] DeleteStudentP5EntryRequest query);

        [Put("/p5-projects/update-student-p5-entry")]
        Task<ApiErrorResult> UpdateStudentP5Entry([Body] UpdateStudentP5EntryRequest query);

        [Put("/p5-projects/update-student-p5-comments")]
        Task<ApiErrorResult> UpdateStudentP5Comments([Body] UpdateStudentP5CommentsRequest query);

        #endregion
    }
}
