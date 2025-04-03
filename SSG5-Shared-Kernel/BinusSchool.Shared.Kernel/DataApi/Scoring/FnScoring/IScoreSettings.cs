using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.AdditionalScoreSettings;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.GeneralScoreEntryPeriod;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreComponent;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.StudentParentScoreView;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.TeacherComment;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectAliasSettings;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ComponentDescription;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.CentralIdea;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.Inquiry;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.UnitsOfInquiry;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.POISetting.POIMappingSettings;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectScoreGroup;
using Refit;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreSummarySetting;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ProgressStatusSettings;
using BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.ScoreSettings.StudentReflectionPeriod;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreSettings : IFnScoring
    {
        #region GeneralScoreEntryPeriod
        [Get("/score-settings/get-subject")]
        Task<ApiErrorResult<IEnumerable<GetSubjectResult>>> GetSubject(GetSubjectRequest query);

        [Get("/score-settings/get-general-score-entry-period-list")]
        Task<ApiErrorResult<IEnumerable<GetGeneralScoreEntryPeriodListResult>>> GetGeneralScoreEntryPeriodList(GetGeneralScoreEntryPeriodListRequest query);

        [Get("/score-settings/get-term-codes")]
        Task<ApiErrorResult<IEnumerable<GetTermCodesResult>>> GetTermCodes(GetTermCodesRequest query);

        [Get("/score-settings/get-all-subjects")]
        Task<ApiErrorResult<IEnumerable<GetAllSubjectsResult>>> GetAllSubjects(GetAllSubjectsRequest query);

        [Post("/score-settings/save-score-entry-period")]
        Task<HttpResponseMessage> SaveScoreEntryPeriod([Body] SaveScoreEntryPeriodRequest param);
        #endregion

        #region StudentParentScoreView
        [Get("/score-settings/get-all-score-view-period")]
        Task<ApiErrorResult<IEnumerable<GetAllScoreViewPeriodResult>>> GetAllScoreViewPeriod(GetAllScoreViewPeriodRequest query);

        [Delete("/score-settings/delete-score-view-period")]
        Task<ApiErrorResult> DeleteScoreViewPeriod([Body] DeleteScoreViewPeriodRequest query);

        [Get("/score-settings/get-all-grade")]
        Task<ApiErrorResult<IEnumerable<GetAllGradeResult>>> GetAllGrade(GetAllGradeRequest query);

        [Post("/score-settings/save-score-view-period")]
        Task<HttpResponseMessage> SaveScoreViewPeriod([Body] SaveScoreViewPeriodRequest param);

        [Get("/score-settings/get-score-view-period-detail")]
        Task<ApiErrorResult<IEnumerable<GetScoreViewPeriodDetailResult>>> GetScoreViewPeriodDetail(GetScoreViewPeriodDetailRequest query);
        #endregion

        #region TeacherComment
        [Get("/score-settings/get-all-teacher-comment")]
        Task<ApiErrorResult<IEnumerable<GetAllTeacherCommentResult>>> GetAllTeacherComment(GetAllTeacherCommentRequest query);

        [Delete("/score-settings/delete-teacher-comment")]
        Task<ApiErrorResult> DeleteTeacherComment([Body] DeleteTeacherCommentRequest query);

        [Get("/score-settings/get-approval-workflow")]
        Task<ApiErrorResult<IEnumerable<GetApprovalWorkflowResult>>> GetApprovalWorkflow(GetApprovalWorkflowRequest query);

        [Post("/score-settings/save-teacher-comment")]
        Task<HttpResponseMessage> SaveTeacherComment([Body] SaveTeacherCommentRequest param);

        [Get("/score-settings/get-teacher-comment-detail")]
        Task<ApiErrorResult<IEnumerable<GetTeacherCommentDetailResult>>> GetTeacherCommentDetail(GetTeacherCommentDetailRequest query);
        #endregion

        #region Score Component
        [Get("/score-settings/get-score-component-subject-list")]
        Task<ApiErrorResult<IEnumerable<GetScoreComponentSubjectListResult>>> GetScoreComponentSubjectList(GetScoreComponentSubjectListRequest query);

        [Get("/score-settings/get-score-component-default-setting-detail")]
        Task<ApiErrorResult<GetScoreComponentDefaultSettingDetailResult>> GetScoreComponentDefaultSettingDetail(GetScoreComponentDefaultSettingDetailRequest query);

        [Get("/score-settings/get-score-component-score-legend")]
        Task<ApiErrorResult<IEnumerable<GetScoreComponentScoreLegendResult>>> GetScoreComponentScoreLegend(GetScoreComponentScoreLegendRequest query);

        [Get("/score-settings/get-score-component-approval-workflow")]
        Task<ApiErrorResult<List<GetScoreComponentApprovalWorkflowResult>>> GetScoreComponentApprovalWorkflow(GetScoreComponentApprovalWorkflowRequest query);

        [Post("/score-settings/save-score-component-setting")]
        Task<ApiErrorResult> SaveScoreComponentSetting([Body] SaveScoreComponentSettingRequest param);

        [Get("/score-settings/get-score-component-list")]
        Task<ApiErrorResult<IEnumerable<GetScoreComponentListResult>>> GetScoreComponentList(GetScoreComponentListRequest query);

        [Get("/score-settings/get-component-detail-for-score-component")]
        Task<ApiErrorResult<GetComponentDetailForScoreComponentResult>> GetComponentDetailForScoreComponent(GetComponentDetailForScoreComponentRequest query);

        [Get("/score-settings/get-sub-component-detail-for-score-component")]
        Task<ApiErrorResult<GetSubComponentDetailForScoreComponentResult>> GetSubComponentDetailForScoreComponent(GetSubComponentDetailForScoreComponentRequest query);

        [Get("/score-settings/get-sub-component-counter-detail-for-score-component")]
        Task<ApiErrorResult<GetSubComponentCounterDetailForScoreComponentResult>> GetSubComponentCounterDetailForScoreComponent(GetSubComponentCounterDetailForScoreComponentRequest query);

        [Post("/score-settings/save-component-for-score-component")]
        Task<ApiErrorResult> SaveComponentForScoreComponent([Body] SaveComponentForScoreComponentRequest param);

        [Post("/score-settings/save-sub-component-for-score-component")]
        Task<ApiErrorResult> SaveSubComponentForScoreComponent([Body] SaveSubComponentForScoreComponentRequest param);

        [Post("/score-settings/save-sub-component-counter-for-score-component")]
        Task<ApiErrorResult> SaveSubComponentCounterForScoreComponent([Body] SaveSubComponentCounterForScoreComponentRequest param);

        [Post("/score-settings/save-score-component-setting-step2")]
        Task<ApiErrorResult> SaveScoreComponentSettingStep2([Body] SaveScoreComponentSettingStep2Request param);

        [Get("/score-settings/get-period-list-for-score-component")]
        Task<ApiErrorResult<IEnumerable<GetPeriodListForScoreComponentResult>>> GetPeriodListForScoreComponent(GetPeriodListForScoreComponentRequest query);

        [Delete("/score-settings/delete-score-component-subject")]
        Task<ApiErrorResult> DeleteScoreComponentSubject([Body] DeleteScoreComponentSubjectRequest query);

        [Delete("/score-settings/delete-component-for-score-component")]
        Task<ApiErrorResult> DeleteComponentForScoreComponent([Body] DeleteComponentForScoreComponentRequest query);

        [Delete("/score-settings/delete-sub-component-for-score-component")]
        Task<ApiErrorResult> DeleteSubComponentForScoreComponent([Body] DeleteSubComponentForScoreComponentRequest query);

        [Delete("/score-settings/delete-sub-component-counter-for-score-component")]
        Task<ApiErrorResult> DeleteSubComponentCounterForScoreComponent([Body] DeleteSubComponentCounterForScoreComponentRequest query);

        [Post("/score-settings/copy-score-component-setting-from-last-ay")]
        Task<ApiErrorResult> CopyScoreComponentSettingFromLastAY([Body] CopyScoreComponentSettingFromLastAYRequest param);

        [Get("/score-settings/get-available-subject-id-for-score-component")]
        Task<ApiErrorResult<IEnumerable<GetAvailableSubjectIDForScoreComponentResult>>> GetAvailableSubjectIDForScoreComponent(GetAvailableSubjectIDForScoreComponentRequest query);

        [Post("/score-settings/save-sub-component-counter-by-subject-id")]
        Task<ApiErrorResult> SaveSubComponentCounterBySubjectID([Body] SaveSubComponentCounterBySubjectIDRequest param);

        [Get("/score-settings/get-score-component-period")]
        Task<ApiErrorResult<IEnumerable<GetScoreComponentByPeriodResult>>> GetScoreComponentByPeriod(GetScoreComponentByPeriodRequest query);
        #endregion

        #region AdditionalScoreSettings
        [Get("/score-settings/additional-score-settings/{id}")]
        Task<ApiErrorResult<GetAdditionalScoreSettingsDetailResult>> GetAdditionalScoreSettingsDetail(string id);

        [Get("/score-settings/get-additional-score-settings-list")]
        Task<ApiErrorResult<IEnumerable<GetAdditionalScoreSettingsListResult>>> GetAdditionalScoreSettingsList(GetAdditionalScoreSettingsListRequest query);

        [Post("/score-settings/add-additional-score-settings")]
        Task<ApiErrorResult> AddAdditionalScoreSettings([Body] AddUpdateAdditionalScoreSettingsRequest query);

        [Put("/score-settings/update-additional-score-settings")]
        Task<ApiErrorResult> UpdateAdditionalScoreSettings([Body] AddUpdateAdditionalScoreSettingsRequest query);

        [Delete("/score-settings/delete-additional-score-settings")]
        Task<ApiErrorResult> DeleteAdditionalScoreSettings([Body] IEnumerable<string> ids);

        [Post("/score-settings/copy-additional-score-settings")]
        Task<ApiErrorResult> CopyAdditionalScoreSettings([Body] CopyAdditionalScoreSettingsRequest param);
        #endregion

        #region Subject Alias Settings
        [Get("/score-settings/get-subject-alias-list")]
        Task<ApiErrorResult<IEnumerable<GetSubjectAliasListResult>>> GetSubjectAliasList(GetSubjectAliasListRequest query);

        [Post("/score-settings/add-update-subject-alias")]
        Task<ApiErrorResult> AddUpdateSubjectAlias([Body] AddUpdateSubjectAliasRequest query);

        [Get("/score-settings/get-subject-alias-settings-list")]
        Task<ApiErrorResult<IEnumerable<GetSubjectAliasSettingsListResult>>> GetSubjectAliasSettingsList(GetSubjectAliasSettingsListRequest query);

        [Post("/score-settings/add-subject-alias-settings")]
        Task<ApiErrorResult> AddSubjectAliasSettings([Body] AddUpdateSubjectAliasSettingsRequest query);

        [Put("/score-settings/update-subject-alias-settings")]
        Task<ApiErrorResult> UpdateSubjectAliasSettings([Body] AddUpdateSubjectAliasSettingsRequest query);

        [Delete("/score-settings/delete-subject-alias-settings")]
        Task<ApiErrorResult> DeleteSubjectAliasSettings([Body] IEnumerable<string> ids);

        [Get("/score-settings/get-subject-alias-settings-detail/{id}")]
        Task<ApiErrorResult<GetSubjectAliasSettingsDetailResult>> GetSubjectAliasSettingsDetail(string id);

        [Post("/score-settings/copy-subject-alias-settings")]
        Task<ApiErrorResult> CopySubjectAliasSettings([Body] CopySubjectAliasSettingsRequest query);
        #endregion

        #region Component Description

        [Get("/score-settings/get-component-description")]
        Task<ApiErrorResult<IEnumerable<GetComponentDescriptionResult>>> GetComponentDescription(GetComponentDescriptionRequest query);

        [Get("/score-settings/get-component-description-detail")]
        Task<ApiErrorResult<IEnumerable<GetComponentDescriptionDetailResult>>> GetComponentDescriptionDetail(GetComponentDescriptionDetailRequest query);

        [Put("/score-settings/save-component-description-detail")]
        Task<HttpResponseMessage> SaveDeleteComponentDescriptionDetail([Body] List<SaveDeleteComponentDescriptionDetailRequest> param);
        #endregion

        #region POI Setting
        #region Central Idea
        [Post("/poi/setting/central-idea")]
        Task<ApiErrorResult> UpdateCentralIdea([Body] UpdateCentralIdeaRequest body);

        [Delete("/poi/setting/central-idea")]
        Task<ApiErrorResult> DeleteCentralIdea([Body] DeleteCentralIdeaRequest body);

        [Put("/poi/setting/central-idea/status")]
        Task<ApiErrorResult> UpdateCentralIdeaStatus([Body] UpdateCentralIdeaStatusRequest body);

        [Get("/poi/setting/central-idea")]
        Task<ApiErrorResult<IEnumerable<GetCentralIdeaResult>>> GetCentralIdea(GetCentralIdeaRequest query);
        #endregion

        #region Units of Inquiry
        [Post("/poi/setting/units-of-inquiry")]
        Task<ApiErrorResult> UpdateUnitsOfInquiry([Body] UpdateUnitsOfInquiryRequest body);

        [Delete("/poi/setting/units-of-inquiry")]
        Task<ApiErrorResult> DeleteUnitsOfInquiry([Body] DeleteUnitsOfInquiryRequest body);

        [Put("/poi/setting/units-of-inquiry/status")]
        Task<ApiErrorResult> UpdateUnitsOfInquiryStatus([Body] UpdateUnitsOfInquiryStatusRequest body);

        [Get("/poi/setting/units-of-inquiry")]
        Task<ApiErrorResult<IEnumerable<GetUnitsOfInquiryResult>>> GetUnitsOfInquiry(GetUnitsOfInquiryRequest query);
        #endregion

        #region Inquiry
        [Post("/poi/setting/inquiry")]
        Task<ApiErrorResult> UpdateInquiry([Body] UpdateInquiryRequest body);

        [Delete("/poi/setting/inquiry")]
        Task<ApiErrorResult> DeleteInquiry([Body] DeleteInquiryRequest body);

        [Put("/poi/setting/inquiry/status")]
        Task<ApiErrorResult> UpdateInquiryStatus([Body] UpdateInquiryStatusRequest body);

        [Get("/poi/setting/inquiry")]
        Task<ApiErrorResult<IEnumerable<GetInquiryResult>>> GetInquiry(GetInquiryRequest query);
        #endregion
        #endregion

        #region POI Mapping

        [Get("/score-settings/get-poi-mapping-settings")]
        Task<ApiErrorResult<IEnumerable<GetPoiMappingSettingsResult>>> GetPOIMappingSettings(GetPOIMappingSettingsRequest query);

        [Delete("/score-settings/delete-poi-mapping-settings")]
        Task<ApiErrorResult> DeletePOIMappingSettings([Body] DeletePOIMappingSettingsRequest query);

        [Put("/score-settings/save-poi-mapping-settings")]
        Task<ApiErrorResult> SavePOIMappingSettings([Body] SavePOIMappingSettingsRequest query);

        [Get("/score-settings/get-list-grade-available-for-uoi")]
        Task<ApiErrorResult<IEnumerable<GetGradeAvailableForUOIResult>>> GetGradeAvailableForUOI(GetGradeAvailableForUOIRequest query);

        [Post("/score-settings/save-multiple-period-poi-mapping-settings")]
        Task<ApiErrorResult> SaveMultiplePeriodPOIMapping([Body] SaveMultiplePeriodPOIMappingSettingsRequest query);

        [Get("/score-settings/get-poi-mapping-settings-detail")]
        Task<ApiErrorResult<GetPOIMappingSettingsDetailResult>> GetPOIMappingSettingsDetail(GetPOIMappingSettingsDetailRequest query);

        [Post("/score-settings/copy-ay-mapping-settings")]
        Task<ApiErrorResult> CopyAYPOIMappingSettings([Body] CopyAYPOIMappingSettingsRequest query);
        #endregion

        #region SubjectScoreGroup
        [Get("/score-settings/get-subject-score-group-detail")]
        Task<ApiErrorResult<GetSubjectScoreGroupDetailResult>> GetSubjectScoreGroupDetail(GetSubjectScoreGroupDetailRequest query);

        [Get("/score-settings/get-subject-score-group-list")]
        Task<ApiErrorResult<IEnumerable<GetSubjectScoreGroupListResult>>> GetSubjectScoreGroupList(GetSubjectScoreGroupListRequest query);

        [Get("/score-settings/get-subject-list-for-subject-score-group")]
        Task<ApiErrorResult<IEnumerable<GetSubjectListForSubjectScoreGroupResult>>> GetSubjectListForSubjectScoreGroup(GetSubjectListForSubjectScoreGroupRequest query);

        [Post("/score-settings/add-update-subject-score-group")]
        Task<ApiErrorResult> AddUpdateSubjectScoreGroup([Body] AddUpdateSubjectScoreGroupRequest query);

        [Delete("/score-settings/delete-subject-score-group")]
        Task<ApiErrorResult> DeleteSubjectScoreGroup([Body] IEnumerable<string> ids);
        #endregion

        #region Setting Score Summary Tab
        [Get("/settings-score-summary-tab/get-setting-score-summary-tab-detail/{id}")]
        Task<ApiErrorResult<GetSettingScoreSummaryTabDetailResult>> GetSettingScoreSummaryTabDetail(string id);

        [Get("/settings-score-summary-tab/get-setting-score-summary-tab")]
        Task<ApiErrorResult<IEnumerable<GetSettingScoreSummaryTabResult>>> GetSettingScoreSummaryTabList(GetSettingScoreSummaryTabRequest query);

        [Post("/settings-score-summary-tab/add-update-setting-score-summary-tab")]
        Task<ApiErrorResult> AddUpdateSettingScoreSummaryTab([Body] AddUpdateSettingScoreSummaryTabRequest query);

        [Delete("/settings-score-summary-tab/delete-setting-score-summary-tab")]
        Task<ApiErrorResult> DeleteSettingScoreSummaryTab([Body] IEnumerable<string> ids);

        [Get("/settings-score-summary-tab/get-setting-score-summary-tab-section")]
        Task<ApiErrorResult<IEnumerable<GetSettingScoreSummaryTabSectionResult>>> GetSettingScoreSummaryTabSection(GetSettingScoreSummaryTabSectionRequest query);

        [Get("/settings-score-summary-tab/get-all-subject-for-score-summary-tab")]
        Task<ApiErrorResult<IEnumerable<GetAllSubjectForScoreSummaryTabSectionSubjectResult>>> GetAllSubjectForScoreSummaryTabSectionSubject(GetAllSubjectForScoreSummaryTabSectionSubjectRequest query);

        [Get("/settings-score-summary-tab/get-score-tab-template")]
        Task<ApiErrorResult<GetScoreTabTemplateResult>> GetScoreTabTemplate(GetScoreTabTemplateRequest query);

        [Post("/settings-score-summary-tab/add-update-setting-score-summary-tab-section")]
        Task<ApiErrorResult> AddUpdateSettingScoreSummaryTabSection([Body] AddUpdateSettingScoreSummaryTabSectionRequest query);

        [Delete("/settings-score-summary-tab/delete-score-summary-tab-section")]
        Task<ApiErrorResult> DeleteScoreSummaryTabSection([Body] DeleteScoreSummaryTabSectionRequest param);

        [Post("/settings-score-summary-tab/copy-score-summary-setting")]
        Task<ApiErrorResult> CopyScoreSummarySetting([Body] CopyScoreSummarySettingRequest query);
        #endregion

        #region Progress Status Setting 
        [Get("/score-settings/get-progress-status-settings-list")]
        Task<ApiErrorResult<IEnumerable<GetProgressStatusSettingsListResult>>> GetProgressStatusSettingsList(GetProgressStatusSettingsListRequest query);

        [Get("/score-settings/get-progress-status-settings-detail")]
        Task<ApiErrorResult<GetProgressStatusSettingsDetailResult>> GetProgressStatusSettingsDetail(GetProgressStatusSettingsDetailRequest query);

        [Get("/score-settings/get-progress-status-settings-by-ay")]
        Task<ApiErrorResult<List<GetProgressStatusSettingsByAYResult>>> GetProgressStatusSettingsByAY(GetProgressStatusSettingsByAYRequest query);

        [Post("/score-settings/add-update-progress-status-pic")]
        Task<ApiErrorResult> AddUpdateProgressStatusPIC([Body] AddUpdateProgressStatusPICRequest query);

        [Post("/score-settings/add-update-progress-status-setting-period")]
        Task<ApiErrorResult> AddUpdateProgressStatusSettingPeriod([Body] AddUpdateProgressStatusSettingPeriodRequest query);

        [Post("/score-settings/add-update-progress-student-status-mapping")]
        Task<ApiErrorResult> AddUpdateProgressStudentStatusMapping([Body] AddUpdateProgressStudentStatusMappingRequest query);

        [Delete("/score-settings/delete-progress-status-setting")]
        Task<ApiErrorResult> DeleteProgressStatusSetting([Body] DeleteProgressStatusSettingRequest param);

        [Post("/score-settings/sync-student-progress-status")]
        Task<ApiErrorResult> SyncStudentProgressStatus([Body] SyncStudentProgressStatusRequest query);
        #endregion

        #region Student Reflection Perriod
        [Get("/score-settings/get-list-student-reflection-period")]
        Task<ApiErrorResult<IEnumerable<GetListStudentReflectionPeriodResult>>> GetListStudentReflectionPeriod(GetListStudentReflectionPeriodRequest query);

        [Post("/score-settings/save-student-reflection-period")]
        Task<ApiErrorResult> SaveStudentReflectionPeriod([Body] SaveStudentReflectionPeriodRequest query);

        [Delete("/score-settings/delete-student-reflection-period")]
        Task<ApiErrorResult> DeleteStudentReflectionPeriod([Body] DeleteStudentReflectionPeriodRequest query);
        #endregion
    }
}
