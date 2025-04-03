using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentApprovalSummary;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentApprovalSummary : IFnStudent
    {
        [Post("/student/StudentApprovalSummary/GetStudentApprovalSummary")]
        Task<ApiErrorResult<IEnumerable<GetStudentApprovalSummaryResult>>> GetStudentApprovalSummary([Body] GetStudentApprovalSummaryRequest param);
    }
}
