using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentEnrollment;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnMovingStudent
{
    public interface IMoveStudentEnrollment : IFnMovingStudent
    {
        [Get("/moving-student/move-student-enrollment")]
        Task<ApiErrorResult<IEnumerable<GetMoveStudentEnrollmentResult>>> GetMoveStudentEnrollment(GetMoveStudentEnrollmentRequest query);

        [Post("/moving-student/move-student-enrollment")]
        Task<ApiErrorResult> AddMoveStudentEnrollment([Body] AddMoveStudentEnrollmentRequest query);

        [Get("/moving-student/move-student-enrollment/{id}")]
        Task<ApiErrorResult<GetDetailMoveStudentEnrollmentResult>> GetDetailMoveStudentEnrollment(string id);

        [Get("/moving-student/move-student-enrollment-history")]
        Task<ApiErrorResult<IEnumerable<GetMoveStudentEnrollmentHistoryResult>>> GetMoveStudentEnrollmentHistory(GetMoveStudentEnrollmentHistoryRequest query);

        [Put("/moving-student/move-student-enrollment-sync")]
        Task<ApiErrorResult> MoveStudentEnrollmentSync([Body] MoveStudentEnrollmentSyncRequest query);

        [Get("/moving-student/move-student-enrollment-subject")]
        Task<ApiErrorResult<IEnumerable<GetSubjectStudentEnrollmentResult>>> GetSubjectStudentEnrollment(GetStudentEnrollmentSubjectRequest query);

        [Post("/moving-student/move-student-enrollment-add")]
        Task<ApiErrorResult<List<string>>> CreateMoveStudentEnrollment([Body] AddMoveStudentEnrollmentRequest query);
    }
}
