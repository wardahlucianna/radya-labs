using Refit;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherByAssignment;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface ITeacherByAssignment : IFnAssignment
    {
        [Post("/teaching/TeacherByAssignment/GetTeacherByGrade")]
        Task<ApiErrorResult<IEnumerable<GetTeacherByGradeResult>>> GetTeacherByGrade([Body] GetTeacherByGradeRequest body);

        [Post("/teaching/TeacherByAssignment/GetTeacherByDepartment")]
        Task<ApiErrorResult<IEnumerable<GetTeacherByDepartmentResult>>> GetTeacherByDepartment([Body] GetTeacherByDepartmentRequest body);
    }
}
