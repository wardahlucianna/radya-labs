using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.MapAttendance;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IMapAttendance : IFnAttendance
    {
        [Get("/map-attendance/level")]
        Task<ApiErrorResult<IEnumerable<GetMapAttendanceLevelResult>>> GetMapAttendanceLevels(GetMapAttendanceLevelRequest param);

        [Get("/map-attendance/detail/{idLevel}")]
        Task<ApiErrorResult<GetMapAttendanceDetailResult>> GetMapAttendanceDetail(string idLevel);

        [Post("/map-attendance/update")]
        Task<ApiErrorResult> UpdateMappingAttendance(AddOrUpdateMappingAttendanceRequest param);
    }
}
