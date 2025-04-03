using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByDept;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IStudentScoreViewByDept : IFnScoring
    {
        [Get("/studentscore/view-byDept-header")]
        Task<ApiErrorResult<IEnumerable<SubComponentCounterVm>>> GetStudentScoreViewHeaderByDept(GetStudentScoreViewHeaderByDeptRequest query);
        
        [Get("/studentscore/view-byDept")]
        Task<ApiErrorResult<IEnumerable<GetStudentScoreViewByDeptResult>>> GetStudentScoreViewByDept(GetStudentScoreViewByDeptRequest query);

    }
}
