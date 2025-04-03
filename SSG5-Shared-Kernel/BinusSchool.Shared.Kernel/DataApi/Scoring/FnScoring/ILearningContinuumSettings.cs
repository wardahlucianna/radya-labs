using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.ExpectedLearningContinuumSettings;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.LearningContinuumPeriod;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings.LearningContinuumCategorySettings;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings.LearningContinuumItemSettings;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings.LearningContinuumTypeSettings;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings.MappingSubjectLearningContinuumSettings;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuumSettings.MasterLearningContinuumSettings.SubjectLearningContinuumSettings;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ILearningContinuumSettings : IFnScoring
    {
        #region Learning Continuum Period Settings 
        [Get("/learningcontinuumSettings/get-learning-continuum-period-settings")]
        Task<ApiErrorResult<IEnumerable<GetLearningContinuumPeriodSettingsResult>>> GetLearningContinuumPeriodSettings(GetLearningContinuumPeriodSettingsRequest query);

        [Post("/learningcontinuumSettings/save-learning-continuum-period-settings")]
        Task<ApiErrorResult> SaveLearningContinuumPeriodSettings([Body] SaveLearningContinuumPeriodSettingsRequest query);

        [Delete("/learningcontinuumSettings/delete-learning-continuum-period-settings")]
        Task<ApiErrorResult> DeleteLearningContinuumPeriodSettings([Body] DeleteLearningContinuumPeriodSettingsRequest query);
        #endregion

        #region Learning Continuum Expected Settings
        [Get("/learningcontinuumSettings/get-expected-learning-continuum-phase")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetExpectedLearningContinuumPhase(GetExpectedLearningContinuumPhaseRequest query);

        [Get("/learningcontinuumSettings/get-expected-learning-continuum-settings")]
        Task<ApiErrorResult<IEnumerable<GetExpectedLearningContinuumSettingsResult>>> GetExpectedLearningContinuumSettings(GetExpectedLearningContinuumSettingsRequest query);

        [Delete("/learningcontinuumSettings/delete-expected-learning-continuum-settings")]
        Task<ApiErrorResult> DeleteExpectedLearningContinuumSettings([Body] DeleteExpectedLearningContinuumSettingsRequest query);

        [Post("/learningcontinuumSettings/copy-ay-expected-learning-continuum-settings")]
        Task<ApiErrorResult> CopyAYExpectedLearningContinuumSettings([Body] CopyAYExpectedLearningContinuumSettingsRequest query);

        [Post("/learningcontinuumSettings/save-expected-learning-continuum-settings")]
        Task<ApiErrorResult> SaveExpectedLearningContinuumSettings([Body] SaveExpectedLearningContinuumSettingsRequest query);
        #endregion

        #region Master Learning Continuum Settings 

        [Get("/learningcontinuumSettings/get-master-learning-continuum-phase")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetMasterLearningContinuumPhase(GetMasterLearningContinuumPhaseRequest query);

        [Get("/learningcontinuumSettings/get-master-learning-continuum-settings")]
        Task<ApiErrorResult<IEnumerable<GetMasterLearningContinuumSettingsResult>>> GetMasterLearningContinuumSettings(GetMasterLearningContinuumSettingsRequest query);

        [Post("/learningcontinuumSettings/save-master-learning-continuum-settings")]
        Task<ApiErrorResult> SaveMasterLearningContinuumSettings([Body] SaveMasterLearningContinuumSettingsRequest query);

        [Delete("/learningcontinuumSettings/delete-master-learning-continuum-settings")]
        Task<ApiErrorResult> DeleteMasterLearningContinuumSettings([Body] IEnumerable<DeleteMasterLearningContinuumSettingsRequest> query);

        [Post("/learningcontinuumSettings/copy-ay-master-learning-continuum-settings")]
        Task<ApiErrorResult> CopyAYMasterLearningContinuumSettings([Body] CopyAYMasterLearningContinuumSettingsRequest query);

        #region Mapping Subject Learning Continuum 

        [Get("/learningcontinuumSettings/get-mapping-subject-learning-continuum-settings")]
        Task<ApiErrorResult<IEnumerable<GetMappingSubjectLearningContinuumSettingsResult>>> GetMappingSubjectLearningContinuumSettings(GetMappingSubjectLearningContinuumSettingsRequest query);

        [Post("/learningcontinuumSettings/delete-subject-learning-continuum-settings")]
        Task<ApiErrorResult> DeleteSubjectLearningContinuumSettings([Body] DeleteSubjectLearningContinuumSettingsRequest query);

        [Post("/learningcontinuumSettings/save-subject-learning-continuum-settings")]
        Task<ApiErrorResult> SaveSubjectLearningContinuumSettings([Body] SaveSubjectLearningContinuumSettingsRequest query);

        #endregion

        #region Learning Continuum Type Settings

        [Get("/learningcontinuumSettings/get-list-learning-continuum-type")]
        Task<ApiErrorResult<IEnumerable<GetLearningContinuumTypeSettingsResult>>> GetListLearningContinuumType(GetLearningContinuumTypeSettingsRequest query);

        [Post("/learningcontinuumSettings/save-learning-continuum-type-settings")]
        Task<ApiErrorResult> SaveLearningContinuumTypeSettings([Body] SaveLearningContinuumTypeSettingsRequest query);

        [Delete("/learningcontinuumSettings/delete-learning-continuum-type-settings")]
        Task<ApiErrorResult> DeleteLearningContinuumTypeSettings([Body] DeleteLearningContinuumTypeSettingsRequest query);

        #endregion

        #region Learning Continuum Category Settings

        [Get("/learningcontinuumSettings/get-list-learning-continuum-category")]
        Task<ApiErrorResult<IEnumerable<GetLearningContinuumCategorySettingsResult>>> GetListLearningContinuumCategory(GetLearningContinuumCategorySettingsRequest query);

        [Post("/learningcontinuumSettings/save-learning-continuum-category-settings")]
        Task<ApiErrorResult> SaveLearningContinuumCategorySettings([Body] SaveLearningContinuumCategorySettingsRequest query);

        [Delete("/learningcontinuumSettings/delete-learning-continuum-category-settings")]
        Task<ApiErrorResult> DeleteLearningContinuumCategorySettings([Body] DeleteLearningContinuumCategorySettingsRequest query);

        #endregion

        #region Learning Continuum Item Settings

        [Get("/learningcontinuumSettings/get-list-learning-continuum-item")]
        Task<ApiErrorResult<IEnumerable<GetLearningContinuumItemSettingsResult>>> GetListLearningContinuumItem(GetLearningContinuumItemSettingsRequest query);

        [Post("/learningcontinuumSettings/save-learning-continuum-item-settings")]
        Task<ApiErrorResult> SaveLearningContinuumItemSettings([Body] SaveLearningContinuumItemSettingsRequest query);

        [Delete("/learningcontinuumSettings/delete-learning-continuum-item-settings")]
        Task<ApiErrorResult> DeleteLearningContinuumItemSettings([Body] DeleteLearningContinuumItemSettingsRequest query);

        #endregion

        #endregion
    }
}
