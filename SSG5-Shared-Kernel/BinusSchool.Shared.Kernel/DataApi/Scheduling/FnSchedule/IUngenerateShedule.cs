using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.UngenerateSchedule;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IUngenerateShedule : IFnSchedule
    {
        [Get("/schedule/ungenerate-schedule/get-lesson-by-grade")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetScheduleLessonsByGrade(GetScheduleLessonsByGradeRequest query);

        [Post("/schedule/ungenerate-schedule/get-lesson-by-grade-v2")]
        Task<ApiErrorResult<IEnumerable<GetScheduleLessonsByGradeV2Result>>> GetScheduleLessonsByGradeV2([Body]GetScheduleLessonsByGradeRequest body);

        [Get("/schedule/ungenerate-schedule/get-lesson-by-student")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetScheduleLessonsByStudent(GetScheduleLessonsByStudentRequest query);

        [Delete("/schedule/ungenerate-schedule/grade")]
        Task<ApiErrorResult> DeleteUngenerateScheduleGrade([Body] DeleteUngenerateScheduleGradeRequest body);

        [Delete("/schedule/ungenerate-schedule/grade-v2")]
        Task<ApiErrorResult> DeleteUngenerateScheduleGradeV2([Body] DeleteUngenerateScheduleGradeV2Request body);

        [Delete("/schedule/ungenerate-schedule/student")]
        Task<ApiErrorResult> DeleteUngenerateScheduleStudent([Body] DeleteUngenerateScheduleStudentRequest body);

        [Put("/schedule/update-generate-schedule/student/homeroom")]
        Task<ApiErrorResult> UpdateGenerateScheduleStudentHomeroom([Body] UpdateGenerateScheduleStudentHomeroomRequest body);

        [Get("/schedule/ungenerate-schedule/get-day-by-grade")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetDayByGarde(GetDayByGardeRequest query);

        [Get("/schedule/ungenerate-schedule/get-asc-name")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetAscNameUngenerateSchedule(GetAscNameUngenerateScheduleRequest query);

        [Get("/schedule/ungenerate-schedule/get-grade")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetGradeUngenerateSchedule(GetGradeUngenerateScheduleRequest query);
    }
}
