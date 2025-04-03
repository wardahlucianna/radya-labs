using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IEmergencyAttendance : IFnStudent
    {
        [Get("/emergency-attendance/summary")]
        Task<ApiErrorResult<IEnumerable<SummaryResult>>> GetSummary(GetSummaryRequest query);

        [Get("/emergency-attendance/unsubmitted")]
        Task<ApiErrorResult<IEnumerable<UnsubmittedStudentResult>>> GetUnsubmittedStudents(GetUnsubmittedStudentsRequest query);

        [Get("/emergency-attendance/submitted")]
        Task<ApiErrorResult<IEnumerable<SubmittedStudentResult>>> GetSubmittedStudents(GetSubmittedStudentsRequest query);

        [Post("/emergency-attendance/submit")]
        Task<ApiErrorResult> Submit([Body] SubmitRequest body);

        [Post("/emergency-attendance/unsubmit")]
        Task<ApiErrorResult> Unsubmit([Body] UnsubmitRequest body);
    }
}
