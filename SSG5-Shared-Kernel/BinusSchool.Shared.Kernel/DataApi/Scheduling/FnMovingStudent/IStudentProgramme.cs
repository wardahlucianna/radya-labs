using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnMovingStudent
{
    public interface IStudentProgramme : IFnMovingStudent
    {
        [Get("/moving-student/student-programme")]
        Task<ApiErrorResult<IEnumerable<GetStudentProgrammeResult>>> GetStudentProgramme(GetStudentProgrammeRequest query);

        [Post("/moving-student/student-programme")]
        Task<ApiErrorResult> AddStudentProgramme([Body] AddStudentProgrammeRequest query);

        [Get("/moving-student/student-programme/{id}")]
        Task<ApiErrorResult<GetDetailStudentProgrammeResult>> GetDetailStudentProgramme(string id);

        [Put("/moving-student/student-programme")]
        Task<ApiErrorResult> UpdateStudentProgramme([Body] UpdateStudentProgrammeRequest query);

        [Delete("/moving-student/student-programme")]
        Task<ApiErrorResult> DeleteStudentProgramme([Body] DeleteStudentProgrammeRequest query);

        [Get("/moving-student/student-programme-history")]
        Task<ApiErrorResult<IEnumerable<GetStudentProgrammeHistoryResult>>> GetStudentProgrammeHistory(GetStudentProgrammeHistoryRequest query);

        [Get("/moving-student/student-programme-student")]
        Task<ApiErrorResult<IEnumerable<GetStudentProgrammeStudentResult>>> GetStudentProgrammeStudent(GetStudentProgrammeStudentRequest query);
    }
}
