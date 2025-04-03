using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination;
using Refit;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface ISubjectCombination : IFnAssignment
    {
        [Get("/subject-combination")]
        Task<ApiErrorResult<GetSubjectCombinationResult>> GetSubjectCombination(GetSubjectCombinationRequest query);

        [Post("/subject-combination")]
        Task<ApiErrorResult> AddSubjectCombination([Body] AddSubjectCombination body);

        [Post("/subject-combination/metadata")]
        Task<ApiErrorResult<List<GetListSubjectCombinationTimetableResult>>> GetSubjectCombinationMetadata([Body]IdCollection param);
    }
}
