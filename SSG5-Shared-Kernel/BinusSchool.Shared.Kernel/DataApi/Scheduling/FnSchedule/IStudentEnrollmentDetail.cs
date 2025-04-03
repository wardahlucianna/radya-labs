using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollmentDetail;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IStudentEnrollmentDetail : IFnSchedule
    {
        [Post("/schedule/student-enrollment-detail/get-list-student")]
        Task<ApiErrorResult<IEnumerable<GetStudentEnrollmentforStudentApprovalSummaryResult>>> GetStudentEnrollmentForStudentApprovalSummary([Body] GetStudentEnrollmentforStudentApprovalSummaryRequest param);
    }
}
