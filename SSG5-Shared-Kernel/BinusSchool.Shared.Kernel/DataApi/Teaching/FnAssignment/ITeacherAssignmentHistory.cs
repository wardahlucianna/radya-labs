using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using System.Threading.Tasks;
using Refit;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignmentHistory;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface ITeacherAssignmentHistory : IFnAssignment
    {
        [Post("/teaching/teacher-assignment-history")]
        Task<ApiErrorResult<IEnumerable<GetTeacherAssignmentHistoryResult>>> GetTeacherAssignmentHistory([Body] GetTeacherAssignmentHistoryRequest query);
    }
}
