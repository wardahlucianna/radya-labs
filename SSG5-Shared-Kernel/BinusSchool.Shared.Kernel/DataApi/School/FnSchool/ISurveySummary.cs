using System.Collections.Generic;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;
using System.Net.Http;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface ISurveySummary : IFnSchool
    {
        #region Survey summary
        [Get("/survey-summary/user-respondent")]
        Task<ApiErrorResult<List<GetSurveySummaryUserRespondentResult>>> GetSurveySummaryUserRespondent(GetSurveySummaryUserRespondentRequest query);

        [Get("/survey-summary")]
        Task<ApiErrorResult<List<GetSurveySummaryResult>>> GetSurveySummary(GetSurveySummaryRequest query);

        [Get("/survey-summary/respondent-detail")]
        Task<ApiErrorResult<List<DetailSurveySummaryRespondentResult>>> DetailSurveySummaryRespondent(DetailSurveySummaryRespondentRequest query);

        [Get("/survey-summary/respondent")]
        Task<ApiErrorResult<List<GetSurveySummaryRespondentResult>>> GetSurveySummaryRespondent(GetSurveySummaryRespondentRequest query);
        #endregion

        #region Survey Summary Download
        [Get("/survey-summary/email")]
        Task<ApiErrorResult> SendEmailSurveySummary(SendEmailSurveySummaryRequest query);

        [Get("/survey-summary/log")]
        Task<ApiErrorResult<GetSurveySummaryLogResult>> GetSurveySummaryLog(GetSurveySummaryLogRequest query);

        [Post("/survey-summary/log")]
        Task<ApiErrorResult> AddAndUpdateSurveySummaryLog([Body] AddAndUpdateSurveySummaryLogRequest body);

        [Get("/survey-summary/download?id={id}")]
        Task<HttpResponseMessage> GetDownloadSurveySummary(string id);
        #endregion
    }
}
