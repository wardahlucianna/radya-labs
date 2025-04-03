using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IStudentEnrollment : IFnSchedule
    {
        [Get("/schedule/student-enrollment/get-list-student")]
        Task<ApiErrorResult<IEnumerable<GetStudentEnrollmentWithGradeResult>>> GetStudentWithGrade(GetStudentEnrollmentWithGradeRequest query);

        [Get("/schedule/student-enrollment")]
        Task<ApiErrorResult<GetStudentEnrollmentResult>> GetStudentEnrollments(GetStudentEnrollmentRequest query);

        [Get("/schedule/student-enrollment/download")]
        Task<HttpResponseMessage> GetDownloadStudentEnrollments(GetStudentEnrollmentRequest query);

        [Put("/schedule/student-enrollment")]
        Task<ApiErrorResult> UpdateStudentEnrollments([Body] UpdateStudentEnrollmentRequest body);

        [Get("/schedule/student-enrollment/student")]
        Task<ApiErrorResult<IEnumerable<GetStudentEnrollmentStudentResult>>> GetStudentEnrollmentsStudent(GetStudentEnrollmentStudentRequest query);
        
        [Get("/schedule/student-enrollment/homeroom")]
        Task<ApiErrorResult<IEnumerable<GetStudentEnrollmentHomeroomResult>>> GetStudentEnrollmentsHomeroom(GetStudentEnrollmentHomeroomRequest query);
        
        [Get("/schedule/student-enrollment/subject")]
        Task<ApiErrorResult<IEnumerable<GetStudentEnrollmentSubjectResult>>> GetStudentEnrollmentsSubject(GetStudentEnrollmentSubjectRequest query);

        [Put("/schedule/student-enrollment-copy")]
        Task<ApiErrorResult> UpdateStudentEnrollmentsCopy([Body] UpdateStudentEnrollmentCopyRequest body);
    }
}
