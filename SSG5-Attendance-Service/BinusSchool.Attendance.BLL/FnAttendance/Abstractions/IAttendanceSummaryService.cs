using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Models;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Models;

namespace BinusSchool.Attendance.FnAttendance.Abstractions
{
    public interface IAttendanceSummaryService
    {
        Task<List<PeriodResult>> GetPeriodAsync(string idAcademicYear,
            string idLevel,
            CancellationToken cancellationToken);

        Task<List<HomeroomStudentEnrollmentResult>> GetHomeroomStudentEnrollmentAsync(string idAcademicYear,
            string idLevel,
            CancellationToken cancellationToken);

        Task<List<HomeroomStudentEnrollmentResult>> GetTrHomeroomStudentEnrollmentAsync(string idAcademicYear,
            string idLevel,
            CancellationToken cancellationToken);

        Task<List<HomeroomTeacherResult>> GetHomeroomTeacherAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken);

        Task<List<ScheduleResult>> GetScheduleAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken);

        Task<List<NonTeachingLoadResult>> GetNonTeachingLoadResultAsync(string idAcademicYear,
            CancellationToken cancellationToken);

        Task<List<DepartmentResult>> GetDepartmentLevelResultAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken);

        Task<StudentStatusResult> GetStudentStatusResultAsync(string idAcademicYear, string idStudent,
            CancellationToken cancellationToken);

        Task<List<AttendanceEntryResult>> GetAttendanceEntryAsync(
            CancellationToken cancellationToken);

        Task<List<ScheduleLessonResult>> GetScheduleLessonAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken);

        Task<List<MappingAttendanceResult>> GetMappingAttendanceAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken);

        Task<List<LessonTeacherResult>> GetLessonTeacherAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken);

        Task<List<AttendanceMappingAttendanceResult>> GetAttendanceMappingAttendanceAsync(string idAcademicYear,
            string idLevel,
            CancellationToken cancellationToken);

        Task<List<CodeWithIdVm>> GetMsAttendanceMappingAttendanceAsync(string idLevel,
            CancellationToken cancellationToken);

        Task<List<CodeWithIdVm>> GetMsMappingAttendanceWorkhabitAsync(string idLevel,
            CancellationToken cancellationToken);

        Task<Dictionary<int, List<HomeroomResult>>> GetHomeroomsGroupedBySemester(string idGrade,
            CancellationToken cancellationToken);

        Task<Dictionary<string, List<AttendanceEntryResult>>> GetAttendanceEntriesGroupedAsync(string[] idSchedules,
            CancellationToken cancellationToken);

        Task<Dictionary<string, List<AttendanceEntryResult>>> GetAttendanceEntriesPendingGroupedAsync(string[] idSchedules,
            CancellationToken cancellationToken);

        Task<List<StudentEnrollmentDto2>> GetStudentEnrolledAsync(string idHomeroom,
            DateTime startAttendanceDt,
            CancellationToken cancellationToken);

        Task<List<StudentStatusDto>> GetStudentStatusesAsync(string[] studentIds, string idAcademicYear,
            CancellationToken cancellationToken);

        Task<List<AttendanceEntryResult>> GetAttendanceEntryByStudentAsync(string idAcademicYear, string idStudent,DateTime startDate, DateTime endDate,
            CancellationToken cancellationToken);

        Task<List<StudentEnrollmentDto2>> GetStudentEnrolledByStudentAsync(string idAcademicYear, string idStudent,
            DateTime startAttendanceDt,
            CancellationToken cancellationToken);

        Task<MsFormula> GetFormulaAsync(string idAcademicYear, string idLevel,
        CancellationToken cancellationToken);

        Task<List<ScheduleLessonResult>> GetScheduleLessonByGradeAsync(string idAcademicYear, string idGrade,
        CancellationToken cancellationToken);

        Task<List<AttendanceEntryResult>> GetAttendanceEntryUnexcusedAbsenceAsync(string idAcademicYear, string idLevel, string idGrade, List<string> ListStudent,
        DateTime startDate, DateTime endDate,CancellationToken cancellationToken);
    }
}
