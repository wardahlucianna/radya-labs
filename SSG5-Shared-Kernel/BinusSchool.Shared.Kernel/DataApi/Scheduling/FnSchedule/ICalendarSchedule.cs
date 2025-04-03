using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.School.FnSchool.Level;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface ICalendarSchedule : IFnSchedule
    {
        [Get("/schedule/calendar/schedule/teacher/academic-year")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetTeacherAcademicYears(GetTeacherAcademicYearsRequest query);

        [Get("/schedule/calendar/schedule/teacher/level")]
        Task<ApiErrorResult<IEnumerable<GetLevelResult>>> GetTeacherLevels(GetTeacherLevelsRequest query);

        [Get("/schedule/calendar/schedule/teacher/grade")]
        Task<ApiErrorResult<IEnumerable<GetGradeResult>>> GetTeacherGrades(GetTeacherGradesRequest query);

        [Get("/schedule/calendar/schedule/teacher/semester")]
        Task<ApiErrorResult<IEnumerable<int>>> GetTeacherSemesters(GetTeacherSemestersRequest query);

        [Get("/schedule/calendar/schedule/teacher/homeroom")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetTeacherHomerooms(GetTeacherHomeroomsRequest query);

        [Get("/schedule/calendar/schedule/teacher/homeroom-v2")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetTeacherHomeroomsV2(GetTeacherHomeroomsRequest query);

        [Get("/schedule/calendar/schedule/teacher")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetTeachers(GetUsersRequest query);

        [Get("/schedule/calendar/schedule/teacher/subject")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetTeacherSubjects(GetUserSubjectsRequest query);

        [Get("/schedule/calendar/schedule/teacher/list-subject-teacher-position")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetListSubjectTeacherPosition(GetUserSubjectDescriptionRequest query);

        [Get("/schedule/calendar/schedule/student")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetStudents(GetUsersRequest query);

        [Get("/schedule/calendar/schedule/student/subject")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetStudentSubjects(GetUserSubjectsRequest query);

        [Get("/schedule/calendar/schedule")]
        Task<ApiErrorResult<IEnumerable<GetCalendarScheduleResult>>> GetSchedules(GetCalendarScheduleRequest query);

        [Get("/schedule/calendar/schedule/download-excel")]
        Task<HttpResponseMessage> DownloadExcelSchedule(DownloadExcelCalendarScheduleRequest query);

        [Get("/schedule/calendar/schedule/download-excel-v2")]
        Task<HttpResponseMessage> DownloadExcelCalendarScheduleV2(DownloadExcelCalendarScheduleRequest query);

        [Get("/schedule/calendar/schedule/student/subject-assignment")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetStudentSubjectsAssignment(GetUserSubjectsRequest query);

        [Get("/schedule/calendar/schedulev2")]
        Task<ApiErrorResult<IEnumerable<GetCalendarScheduleV2Result>>> GetCalendarSchedulesV2(GetCalendarScheduleV2Request query);

        [Get("/schedule/calendar/schedule/level")]
        Task<ApiErrorResult<IEnumerable<GetCalendarScheduleLevelResult>>> GetCalendarScheduleLevel(GetCalendarScheduleLevelRequest query);
    }
}
