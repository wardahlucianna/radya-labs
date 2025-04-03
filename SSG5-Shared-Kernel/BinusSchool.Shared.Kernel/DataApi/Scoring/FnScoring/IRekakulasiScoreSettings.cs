using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.SendEmail;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scoring.FnScoring.RekakulasiScoreSettings;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IRekakulasiScoreSettings : IFnScoring
    {
        [Post("/scoring/rekakulasiscoresettings/update-rekakulasi-score-setting")]
        Task<ApiErrorResult> UpdateRekakulasiScoreSetting(UpdateRekakulasiScoreSettingRequest query);
    }
}
