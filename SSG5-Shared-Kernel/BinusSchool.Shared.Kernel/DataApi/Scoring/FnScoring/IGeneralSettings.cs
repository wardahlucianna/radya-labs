using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.GeneralSettings.CounterCategorySettings;
using BinusSchool.Data.Model.Scoring.FnScoring.GeneralSettings.ScoreOptionSettings;

using Refit;
namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IGeneralSettings : IFnScoring
    {
        #region Counter Category Settings
        [Get("/general-settings/get-counter-category-list")]
        Task<ApiErrorResult<IEnumerable<GetCounterCategoryListResult>>> GetCounterCategoryList(GetCounterCategoryListRequest req);

        [Post("/general-settings/add-counter-category")]
        Task<ApiErrorResult> AddcounterCategoryHandler([Body] AddCounterCategoryRequest query);

        [Delete("/general-settings/delete-counter-category")]
        Task<ApiErrorResult> DeletecounterCategoryHandler([Body] DeleteCounterCategoryRequest query);
        #endregion

        #region Score Option Settings
        [Get("/general-settings/get-score-option-settings-list")]
        Task<ApiErrorResult<IEnumerable<GetScoreOptionSettingsListResult>>> GetScoreSettingsList(GetScoreOptionSettingsListRequest req);

        [Delete("/general-settings/delete-score-option-settings")]
        Task<ApiErrorResult> DeleteScoreOptionSettings([Body] DeleteScoreOptionSettingsRequest query);

        [Get("/general-settings/get-score-option-details")]
        Task<ApiErrorResult<GetScoreOptionsResult>> GetScoreOptionSettingsDetail(GetScoreOptionSettingsDetailRequest req);

        [Post("/general-settings/create-update-score-option-settings")]
        Task<ApiErrorResult> CreateUpdateScoreOptionSettings([Body] CreateUpdateScoreOptionSettingsRequest query);
        #endregion
    }
}
