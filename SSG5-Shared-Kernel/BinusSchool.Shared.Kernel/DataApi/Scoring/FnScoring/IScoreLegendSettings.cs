using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreComponent;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.ScoreLegendSettings;
using BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreLegendSettings : IFnScoring
    {
        [Get("/scorelegendsettings/get-score-legend")]
        Task<ApiErrorResult<IEnumerable<GetScoreLegendSettingsResult>>> GetScoreLegendSettings(GetScoreLegendSettingsRequest query);

        [Delete("/scorelegendsettings/delete-score-legend")]
        Task<ApiErrorResult> DeleteScoreLegendSettings([Body] IEnumerable<string> ids);

        [Post("/scorelegendsettings/add-score-legend")]
        Task<ApiErrorResult> AddScoreLegendSettings([Body] AddScoreLegendSettingsRequest query);

        [Put("/scorelegendsettings/update-score-legend")]
        Task<ApiErrorResult> UpdateScoreLegendSettings([Body] UpdateScoreLegendSettingsRequest query);

        [Get("/scorelegendsettings/get-score-legend-detail")]
        Task<ApiErrorResult<GetScoreLegendSettingsDetailResult>> GetScoreLegendSettingsDetail(GetScoreLegendSettingsDetailRequest query);

        [Post("/scorelegendsettings/copy-score-legend")]
        Task<ApiErrorResult> CopyScoreLegendScoreSettings([Body] CopyScoreLegendScoreSettingsRequest query);
    }
}
