using System.Threading.Tasks;
using BinusSchool.Data.Models.Binusian.BinusSchool.AttendanceLog;
using Refit;

namespace BinusSchool.Data.Apis.Binusian.BinusSchool
{
    public interface IAttendanceLog
    {
        [Post("/binusschool/attendancelog")]
        Task<GetAttendanceLogResult> GetAttendanceLogs([Header("Authorization")] string bearerToken, [Body] GetAttendanceLogRequest body);
    }
}
