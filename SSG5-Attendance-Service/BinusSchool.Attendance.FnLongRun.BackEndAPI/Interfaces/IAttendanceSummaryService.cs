using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Models;

namespace BinusSchool.Attendance.FnLongRun.Interfaces
{
    public interface IAttendanceSummaryService
    {
        Task<string> GetSchoolNameAsync(string idSchool, CancellationToken cancellationToken);
        Task<string> GetActiveAcademicYearAsync(string idSchool, CancellationToken cancellationToken);
        Task DeleteTermByAcademicYearAsync(string academicYear, CancellationToken cancellationToken);

        Task<List<GradeDto>> GetGradesAsync(string idSchool, string idAcademicYear,
            CancellationToken cancellationToken);

        Task<List<string>> GetStudentsByGradeAsync(string idGrade, CancellationToken cancellationToken);
        Task<int> GetTotalStudentByGradeAsync(string idGrade, CancellationToken cancellationToken);
        Task<List<PeriodDto>> GetPeriodsAsync(string idGrade, CancellationToken cancellationToken);
        Task<List<Summary>> GetSummaryPerGradeAsync(string idGrade, CancellationToken cancellationToken);

        Task<List<Summary>> GetSummaryPerGradeIncludeEntriesAsync(string idGrade, string idSchool,
            List<PeriodDto> periods,
            List<MsMappingAttendance> mappingAttendances,
            List<MsMappingAttendanceWorkhabit> mappingAttendanceWorkhabits,
            CancellationToken cancellationToken);

        Task<Entry> GetEntryAsync(string id, CancellationToken cancellationToken);
        Task<List<Entry>> GetEntriesAsync(List<string> listId, CancellationToken cancellationToken);
        List<Entry> GetEntries(List<string> listId);

        Task<List<MsAttendanceMappingAttendance>> GetAttendanceMappingAttendanceAsync(
            CancellationToken cancellationToken);

        Task<List<MsMappingAttendance>> GetMappingAttendanceAsync(CancellationToken cancellationToken);

        Task<List<MsMappingAttendanceWorkhabit>>
            GetMappingAttendanceWorkhabitAsync(CancellationToken cancellationToken);

        Task<List<MsSchoolMappingEA>> GetSchoolMappingEaAsync(CancellationToken cancellationToken);
    }
}
