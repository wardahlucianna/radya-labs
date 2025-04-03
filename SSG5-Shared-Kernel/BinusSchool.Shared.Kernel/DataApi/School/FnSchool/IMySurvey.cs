using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MySurvey;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IMySurvey : IFnSchool
    {
        [Get("/my-survey")]
        Task<ApiErrorResult<IEnumerable<GetMySurveyResult>>> GetMySurveys(GetMySurveyRequest request);

        [Get("/my-survey/detail")]
        Task<ApiErrorResult<GetMySurveyDetailResult>> GetMySurveyDetail(GetMySurveyDetailRequest request);

        [Post("/my-survey")]
        Task<ApiErrorResult> AddMySurvey([Body] AddMySurveyRequest body);

        [Put("/my-survey")]
        Task<ApiErrorResult> UpdateMySurvey([Body] UpdateMySurveyRequest body);

        [Delete("/my-survey")]
        Task<ApiErrorResult> DeleteMySurvey([Body] IEnumerable<string> ids);

        [Get("/my-survey/link-publish-survey")]
        Task<ApiErrorResult<GetMySurveyResult>> GetMySurveyLinkSurvey(GetMySurveyLinkSurveyRequest request);
    }
}
