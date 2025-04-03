using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.LHAndCA;
using Refit;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface ITeacherAssignmentLHAndCA : IFnAssignment
    {
        [Get("/assignment/teacher/lh-ca")]
        Task<ApiErrorResult<IEnumerable<GetAssignLHAndCAResult>>> GetAssignmentLHAndCA(GetAssignLHAndCARequest query);

        [Get("/assignment/teacher/lh-ca/detail")]
        Task<ApiErrorResult<GetAssignLHAndCADetailResult>> GetAssignmentLHAndCADetail(GetAssignLHAndCADetailRequest query);

        [Post("/assignment/teacher/lh-ca")]
        Task<ApiErrorResult> CreateOrUpdateAssignmentLHAndCA([Body] AddAssignLHAndCARequest body);
    }
}
