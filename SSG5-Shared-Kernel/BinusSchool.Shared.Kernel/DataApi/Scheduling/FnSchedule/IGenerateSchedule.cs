using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.GenerateSchedule;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IGenerateSchedule : IFnSchedule
    {
        [Get("/schedule/generate-schedule/get-classids-grade-student")]
        Task<ApiErrorResult<GetClassIDByGradeAndStudentResult>> GetLessons(GetClassIDByGradeAndStudentRequest query);

        [Get("/schedule/generate-schedule/get-detail-by-mount-year")]
        Task<ApiErrorResult<List<GetDetailGenerateScheduleResult>>> GetDetailByMountYears([Query] GetDetailGenerateScheduleRequest query);

        [Get("/schedule/generate-schedule/get-detail-by-month-year-v2")]
        Task<ApiErrorResult<List<GetDetailGenerateScheduleResult>>> GetDetailByMonthYearsV2([Query] GetDetailGenerateScheduleRequest query);

        [Get("/schedule/generate-schedule/get-generate-schedule-with-classid")]
        Task<ApiErrorResult<GetClassIDByGradeAndStudentResult>> GetGenerateScheduleWithClassID([Query] GetGenerateScheduleWithClassIDRequest query);

        [Get("/schedule/generate-schedule/get-detail-by-date")]
        Task<ApiErrorResult<List<GetDetailGenerateScheduleResult>>> GetDetailByDate([Query] GetDetailGenerateScheduleRequest query);

        [Get("/schedule/generate-schedule/get-detail-by-date-v2")]
        Task<ApiErrorResult<List<GetDetailGenerateScheduleResult>>> GetDetailByDateV2([Query] GetDetailGenerateScheduleRequest query);

        [Post("/schedule/generate-schedule")]
        Task<ApiErrorResult> AddGenerate([Body] AddGenerateScheduleRequest body);

        [Get("/schedule/generate-schedule/grade")]
        Task<ApiErrorResult<IEnumerable<GetGenerateScheduleGradeResult>>> GetGenerateScheduleDetailGrade(GetGenerateScheduleGradeRequest query);

        [Get("/schedule/generate-schedule/grade-v2")]
        Task<ApiErrorResult<IEnumerable<GetGenerateScheduleGradeResult>>> GetGenerateScheduleGradeV2(GetGenerateScheduleGradeRequest query);

        [Get("/schedule/generate-schedule/student")]
        Task<ApiErrorResult<IEnumerable<GetGeneratedScheduleStudentResult>>> GetGenerateScheduleDetailStudent(GetGenerateScheduleStudentRequest query);

        [Get("/schedule/generate-schedule/grade-history")]
        Task<ApiErrorResult<IEnumerable<GetGenerateScheduleGradeHistoryResult>>> GetGenerateScheduleGradeHistory(GetGenerateScheduleGradeHistoryRequest query);

        [Get("/schedule/generate-schedule/grade-history-v2")]
        Task<ApiErrorResult<IEnumerable<GetGenerateScheduleGradeHistoryResult>>> GetGenerateScheduleGradeHistoryV2(GetGenerateScheduleGradeHistoryRequest query);

        [Get("/schedule/generate-schedule/check/is-there-job-run")]
        Task<ApiErrorResult> CheckIsJobRunning([Query] StartGeneratedScheduleProcessRequest model);

        [Post("/schedule/generate-schedule/start-process")]
        Task<ApiErrorResult<string>> StartProcess([Body] StartGeneratedScheduleProcessRequest model);

        [Put("/schedule/generate-schedule/finish-process")]
        Task<ApiErrorResult> FinishProcess([Body] FinishGeneratedScheduleProcessRequest model);

        [Get("/schedule/generate-schedule/grade-has-generated-schedule")]
        Task<ApiErrorResult<IEnumerable<GetGradeResult>>> GetGradeHasGeneratedSchedule(GetGradeRequest query);

        [Get("/schedule/generate-schedule/grade-has-generated-schedule-v2")]
        Task<ApiErrorResult<IEnumerable<GetGradeResult>>> GetGradesHasGeneratedScheduleV2(GetGradeRequest query);

        [Get("/schedule/generate-schedule/get-grade-by-asc-timetable")]
        Task<ApiErrorResult<IEnumerable<GetGradeByAscTimetableResult>>> GetGradeByAscTimetable(GetGradeByAscTimetableRequest query);

        [Get("/schedule/generate-schedule/get-week-by-grade-subject")]
        Task<ApiErrorResult<IEnumerable<GetWeekByGradeSubjectResult>>> GetWeekByGradeSubject(GetWeekByGradeSubjectRequest query);
    }
}
