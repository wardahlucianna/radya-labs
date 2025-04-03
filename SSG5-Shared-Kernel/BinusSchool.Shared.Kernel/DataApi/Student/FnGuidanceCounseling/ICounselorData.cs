using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData;
using Refit;

namespace BinusSchool.Data.Api.Student.FnGuidanceCounseling
{
    public interface ICounselorData : IFnGuidanceCounseling
    {
        [Get("/guidance-counseling/counselor-data")]
        Task<ApiErrorResult<IEnumerable<GetCounselorDataResult>>> GetCounselorData(GetCounselorDataRequest query);
              
        [Get("/guidance-counseling/counselor-data/{id}")]
        Task<ApiErrorResult<GetDetailCounselorDataResult>> GetDetailCounselorData(string id);

        [Post("/guidance-counseling/counselor-data")]
        Task<ApiErrorResult> AddCounselorData([Body] AddCounselorDataRequest body);

        [Put("/guidance-counseling/counselor-data")]
        Task<ApiErrorResult> UpdateCounselorData([Body] UpdateCounselorDataRequest body);

        [Delete("/guidance-counseling/counselor-data")]
        Task<ApiErrorResult> DeleteCounselorData([Body] IEnumerable<string> ids);

        [Get("/guidance-counseling/counselor-data/used-grade/{id}")]
        Task<ApiErrorResult<List<string>>> GetUsedGrade(string id);

        [Get("/guidance-counseling/counselor-data-level-grade")]
        Task<ApiErrorResult<List<GetLevelGradeByConcellorResult>>> GetLevelGradeByConcellor(GetLevelGradeByConcellorRequest query);
    }
}
