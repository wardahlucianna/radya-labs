using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.PrivilegeEntryScore;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IClassPrivilege : IFnScoring
    {
        [Post("/scoring/class-privilege")]
        Task<ApiErrorResult<IEnumerable<GetPrivilegeEntryScoreByPositionResult>>> GetClassPrivilegeforEntryScore([Body] GetPrivilegeEntryScoreByPositionRequest query);

        [Post("/scoring/class-privilege-new")]
        Task<ApiErrorResult<IEnumerable<GetPrivilegeEntryScoreByPositionNewResult>>> GetClassPrivilegeforEntryScoreNew([Body] GetPrivilegeEntryScoreByPositionRequest query);

        [Get("/scoring/getclass-for-cla")]
        Task<ApiErrorResult<IEnumerable<GetClassIDCLAResult>>> GetStudentScoreViewByDept(GetClassIDCLARequest query);

    }
}
