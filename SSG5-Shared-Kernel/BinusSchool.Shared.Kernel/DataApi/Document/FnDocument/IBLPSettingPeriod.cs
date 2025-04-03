using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.BLPSettingPeriod;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IBLPSettingPeriod : IFnDocument
    {
        [Get("/blp/setting-period")]
        Task<ApiErrorResult<IEnumerable<GetBLPSettingResult>>> GetBLPSettingPeriod(GetBLPSettingRequest body);

        [Get("/blp/setting-period/{id}")]
        Task<ApiErrorResult<GetBLPSettingDetailResult>> GetBLPSettingPeriodDetail(string id);

        [Post("/blp/setting-period")]
        Task<ApiErrorResult> AddBLPSettingPeriod([Body] AddBLPSettingPeriodRequest body);

        [Put("/blp/setting-period")]
        Task<ApiErrorResult> UpdateBLPSettingPeriod([Body] UpdateBLPSettingPeriodRequest body);

        [Delete("/blp/setting-period")]
        Task<ApiErrorResult> DeleteBLPSettingPeriod([Body] IEnumerable<string> ids);

        [Get("/blp/settingperiod/get-grade")]
        Task<ApiErrorResult<IEnumerable<GetGradeSurveyPeriodResult>>> GetGradeSurveyPeriod(GetGradeSurveyPeriodRequest body);

        [Get("/blp/settingperiod/get-period-by-grade")]
        Task<ApiErrorResult<GetSurveyPeriodByGradeResult>> GetSurveyPeriodByGrade(GetSurveyPeriodByGradeRequest body);

    }
}
