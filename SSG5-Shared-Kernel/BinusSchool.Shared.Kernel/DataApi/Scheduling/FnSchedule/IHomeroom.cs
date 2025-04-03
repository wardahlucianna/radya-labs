using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IHomeroom : IFnSchedule
    {
        [Get("/schedule/homeroom")]
        Task<ApiErrorResult<IEnumerable<GetHomeroomResult>>> GetHomerooms(GetHomeroomRequest query);

        [Get("/schedule/homeroom/{id}")]
        Task<ApiErrorResult<GetHomeroomDetailResult>> GetHomeroomDetail(string id);

        [Post("/schedule/homeroom")]
        Task<ApiErrorResult> AddHomeroom([Body] AddHomeroomRequest body);

        [Put("/schedule/homeroom")]
        Task<ApiErrorResult> UpdateHomeroom([Body] UpdateHomeroomRequest body);

        [Delete("/schedule/homeroom")]
        Task<ApiErrorResult> DeleteHomeroom([Body] IEnumerable<string> ids);

        [Get("/schedule/homeroom-by-student")]
        Task<ApiErrorResult<IEnumerable<GetHomeroomByStudentResult>>> GetHomeroomByStudents(GetHomeroomByStudentRequest query);

        [Post("/schedule/homeroom-move")]
        Task<ApiErrorResult> AddMoveStudentHomeroom([Body] AddMoveHomeroomRequest body);
        [Get("/schedule/homeroom-by-subject-teacher")]
        Task<ApiErrorResult<IEnumerable<GetHomeroomByTeacherResult>>> GetHomeroomsBySubjectTeacher(GetHomeroomByTeacherRequest query);

        [Post("/schedule/homeroom-by-level-grade")]
        Task<ApiErrorResult<IEnumerable<GetHomeroomByLevelGradeResult>>> GetHomeroomByLevelGrade([Body] GetHomeroomByLevelGradeRequest body);

        [Post("/schedule/homeroom-copy")]
        Task<ApiErrorResult> AddHomeroomCopy([Body] AddHomeroomCopyRequest body);
    }
}
