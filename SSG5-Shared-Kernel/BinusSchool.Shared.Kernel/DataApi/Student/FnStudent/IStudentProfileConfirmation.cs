using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentProfileConfirmation;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentProfileConfirmation : IFnStudent
    {
        [Get("/student/profile_confirmation")]
        Task<ApiErrorResult<IEnumerable<GetStudentProfileConfirmationResult>>> GetStudentProfileConfirmation(GetStudentProfileConfirmationRequest query);

        [Get("/student/profile_confirmation/{id}")]
        Task<ApiErrorResult<IEnumerable<GetStudentProfileConfirmationResult>>> GetStudentProfileConfirmationDetail(string id);

        [Post("/student/profile_confirmation")]
        Task<ApiErrorResult> AddStudentProfileConfirmation([Body] AddStudentProfileConfirmationRequest body);

        [Put("/student/profile_confirmation")]
        Task<ApiErrorResult> UpdateStudentProfileConfirmation([Body] AddStudentProfileConfirmationRequest body);

        [Delete("/student/profile_confirmation")]
        Task<ApiErrorResult> DeleteStudentProfileConfirmation([Body] IEnumerable<string> ids);
    }
}
