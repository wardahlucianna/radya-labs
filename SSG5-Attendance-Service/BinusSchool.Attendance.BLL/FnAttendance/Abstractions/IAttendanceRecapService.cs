using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Attendance.FnAttendance.Models;
using System.Threading.Tasks;
using System.Threading;

namespace BinusSchool.Attendance.FnAttendance.Abstractions
{
    public interface IAttendanceRecapService
    {
        Task<List<HomeroomStudentEnrollmentResult>> GetHomeroomStudentEnrollmentAsync(
            string idAcademicYear,
            string idLevel,
            string idStudent,
            CancellationToken cancellationToken);

        Task<List<HomeroomStudentEnrollmentResult>> GetTrHomeroomStudentEnrollmentAsync(
           string idAcademicYear,
           string idLevel,
           string idStudent,
           CancellationToken cancellationToken);

        Task<List<PeriodResult>> GetPeriodAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken);

        Task<List<ScheduleLessonResult>> GetScheduleLessonAsync(string idAcademicYear, string idLevel, List<string> lessons, DateTime? startDate, DateTime? endDate,
            CancellationToken cancellationToken);
    }
}
