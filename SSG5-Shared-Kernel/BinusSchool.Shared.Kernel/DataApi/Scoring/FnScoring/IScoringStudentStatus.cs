using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.StudentStatus;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IScoringStudentStatus : IFnScoring
    {
        [Post("/StudentStatus/CheckStudentStatusPerTerm")]
        Task<ApiErrorResult<CheckStudentStatusPerTermResult>> CheckStudentStatusPerTerm([Body] CheckStudentStatusPerTermRequest query);

        [Post("/StudentStatus/CheckStudentStatusPerSemester")]
        Task<ApiErrorResult<CheckStudentStatusPerSemesterResult>> CheckStudentStatusPerSemester([Body] CheckStudentStatusPerSemesterRequest query);

        [Post("/StudentStatus/CheckStudentStatusHomeroomEnrollment")]
        Task<ApiErrorResult<CheckStudentStatusHomeroomEnrollmentResult>> CheckStudentStatusHomeroomEnrollment([Body] CheckStudentStatusHomeroomEnrollmentRequest query);
    }
}
