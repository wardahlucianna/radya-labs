using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentSubject;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom;

namespace BinusSchool.Data.Api.Scheduling.FnMovingStudent
{
    public interface IMoveStudentHomeroom : IFnMovingStudent
    {
        [Get("/moving-student/move-student-homeroom/homeroom-new")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetHomeroomNewMoveStudentHomeroom(GetHomeroomNewMoveStudentHomeroomRequest query);

        [Get("/moving-student/move-student-homeroom/student")]
        Task<ApiErrorResult<IEnumerable<GetStudentMoveStudentHomeroomResult>>> GetStudentMoveStudentHomeroom(GetStudentMoveStudentHomeroomRequest query);

        [Post("/moving-student/move-student-homeroom")]
        Task<ApiErrorResult> AddStudentMoveStudentHomeroom([Body]AddStudentMoveStudentHomeroomRequest body);

        [Get("/moving-student/move-student-homeroom/history")]
        Task<ApiErrorResult<IEnumerable<HistoryMoveStudentHomeroomResult>>> HistoryMoveStudentHomeroom(HistoryMoveStudentHomeroomRequest query);

        [Put("/moving-student/move-student-homeroom/sync")]
        Task<ApiErrorResult> MoveHomeroomSync([Body] MoveHomeroomSyncRequest body);
    }
}
