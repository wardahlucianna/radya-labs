using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSubject.Department;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.HODAndSH;
using Refit;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface ITeacherAssignmentHODAndSH : IFnAssignment
    {
        [Get("/assignment/teacher/hod-sh")]
        Task<ApiErrorResult<IEnumerable<GetAssignHODAndSHResult>>> GetTeacherAssignmentHODAndSH(GetDepartmentRequest body);

        [Get("/assignment/teacher/hod-sh/detail")]
        Task<ApiErrorResult<GetAssignHODAndSHDetailResult>> GetTeacherAssignmentHODAndSHDetail(GetAssignHODAndSHDetailRequest body);

        [Post("/assignment/teacher/hod-sh")]
        Task<ApiErrorResult> AddTeacherAssignmentHODAndSH([Body] AddAssignHODAndSHRequest body);

        [Delete("/assignment/teacher/hod-sh")]
        Task<ApiErrorResult> DeleteTeacherAssignmentHODAndSH([Body] IEnumerable<string> ids);
    }
}
