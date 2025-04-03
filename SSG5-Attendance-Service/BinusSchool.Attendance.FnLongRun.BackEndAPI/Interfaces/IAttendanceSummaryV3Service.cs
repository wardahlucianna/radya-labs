using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Persistence.AttendanceDb.Models;

namespace BinusSchool.Attendance.FnLongRun.Interfaces
{
    public interface IAttendanceSummaryV3Service
    {
        Task<string> GetSchoolNameAsync(string idSchool, CancellationToken cancellationToken);
        Task<string> GetActiveAcademicYearAsync(string idSchool, CancellationToken cancellationToken);

        Task<List<GradeDto>> GetGradesAsync(string idSchool, string idAcademicYear,
            CancellationToken cancellationToken);

        Task<List<PeriodDto>> GetPeriodsAsync(string idGrade, CancellationToken cancellationToken);
        Task<List<StudentDto>> GetStudentEnrolledBy(string idHomeroom, CancellationToken cancellationToken);

        /// <summary>
        /// Grouped by id lesson
        /// </summary>
        /// <param name="idHomeroom"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Dictionary<string, List<StudentDto>>> GetStudentEnrolledGroupByIdLesson(string idHomeroom,
            CancellationToken cancellationToken);

        Task<List<HomeroomDto>> GetHomeroomsAsync(string idGrade, CancellationToken cancellationToken);

        Task<Dictionary<int, List<HomeroomDto>>> GetHomeroomsGroupedBySemester(string idGrade,
            CancellationToken cancellationToken);

        Task DeleteTermByAcademicYearAsync(string idAcademicYear, CancellationToken cancellationToken);

        Task<List<ScheduleDto>> GetScheduleAsync(string idAcademicYear, string idGrade,
            CancellationToken cancellationToken);

        Task<List<ScheduleDto>> GetScheduleAsync(string idAcademicYear, string idGrade,
            DateTime start, DateTime end,
            CancellationToken cancellationToken);

        Task<List<ScheduleDto>> GetScheduleCancelAsync(string idAcademicYear, string idGrade,
            DateTime start, DateTime end,
            CancellationToken cancellationToken);

        Task<List<AttendanceDto>> GetAttendanceEntriesAsync(string[] idSchedules, CancellationToken cancellationToken);

        Task<Dictionary<string, List<AttendanceDto>>> GetAttendanceEntriesGroupedAsync(string[] idSchedules,
            CancellationToken cancellationToken);

        Task<List<StudentEnrollmentDto>> GetStudentEnrolled(string idHomeroom,
            DateTime startAttendanceDt,
            CancellationToken cancellationToken);

        Task<List<StudentStatusDto>> GetStudentStatusesAsync(string[] studentIds, string idAcademicYear,
            DateTime lastPeriodDt, CancellationToken cancellationToken);
    }
}
