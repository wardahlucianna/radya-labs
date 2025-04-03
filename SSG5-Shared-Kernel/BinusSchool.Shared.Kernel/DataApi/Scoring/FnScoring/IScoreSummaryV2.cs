using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoreSummaryV2 : IFnScoring
    {
        [Post("/ScoreSummaryV2/MasterGenerateScoreSummary")]
        Task<ApiErrorResult<IEnumerable<MasterGenerateScoreSummaryResult>>> MasterGenerateScoreSummary(MasterGenerateScoreSummaryRequest query);

        [Get("/ScoreSummaryV2/GetScoreSummaryTabSetting")]
        Task<ApiErrorResult<IEnumerable<GetScoreSummaryTabSettingResult>>> GetScoreSummaryTabSetting(GetScoreSummaryTabSettingRequest query);
    }
}
