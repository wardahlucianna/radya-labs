using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject;

namespace BinusSchool.Data.Api.Scheduling.FnMovingStudent
{
    public interface IMoveStudentSubject : IFnMovingStudent
    {
        [Get("/moving-student/move-student-subject/homeroom")]
        Task<ApiErrorResult<IEnumerable<GetHomeroomMoveStudentSubjectResult>>> GetHomeroomMoveStudentSubject(GetHomeroomMoveStudentSubjectRequest query);

        [Get("/moving-student/move-student-subject/subject")]
        Task<ApiErrorResult<IEnumerable<GetSubjectMoveStudentSubjectResult>>> GetSubjectMoveStudentSubject(GetSubjectMoveStudentSubjectRequest query);

        [Get("/moving-student/move-student-subject/student")]
        Task<ApiErrorResult<IEnumerable<GetStudentMoveStudentSubjectResult>>> GetStudentMoveStudentSubject(GetStudentMoveStudentSubjectRequest query);

        [Post("/moving-student/move-student-subject")]
        Task<ApiErrorResult> AddMoveStudentSubject([Body] AddMoveStudentSubjectRequest body);
    }
}
