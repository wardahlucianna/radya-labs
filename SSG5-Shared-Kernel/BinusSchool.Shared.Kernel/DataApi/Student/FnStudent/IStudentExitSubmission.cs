using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitSubmission;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentExitSubmission : IFnStudent
    {
        [Get("/student/student-exit-submission")]
        Task<ApiErrorResult<IEnumerable<GetStudentExitSubmissionResult>>> GetStudentExitSubmissions(GetStudentExitSubmissionRequest query);

        [Put("/student/student-exit-submission")]
        Task<ApiErrorResult> UpdateStudentExitSubmission([Body] UpdateStudentExitSubmissionRequest body);
    }
}
