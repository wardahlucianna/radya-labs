using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace BinusSchool.Data.Api.Attendance.FnLongRun
{
    public interface ISchoolEventAttendanceService
    {
        Task<string> GetSchoolEventAttendance(string idSchool, bool halfDay, CancellationToken cancellationToken);
    }
}
