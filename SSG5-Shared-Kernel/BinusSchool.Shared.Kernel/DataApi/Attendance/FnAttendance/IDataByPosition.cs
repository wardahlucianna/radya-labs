using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.DataByPosition;
using BinusSchool.Data.Model.Attendance.FnAttendance.GradeByPosition;
using BinusSchool.Data.Model.Attendance.FnAttendance.HomeroomByPosition;
using BinusSchool.Data.Model.Attendance.FnAttendance.LevelByPosition;
using BinusSchool.Data.Model.Scoring.FnScoring.Filter;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IDataByPosition : IFnAttendance
    {
        [Get("/attendance/level-by-position")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetLevelByPosition(LevelByPositionRequest query);
        [Get("/attendance/grade-by-position")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetGradeByPosition(GradeByPositionRequest query);
        [Get("/attendance/homeroom-by-position")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetHomeroomByPosition(HomeroomByPositionRequest query);
        [Get("/attendance/get-list-filter-attendance")]
        Task<ApiErrorResult<GetListFilterScoringResult>> GetListFilterAttendance(GetListFilterAttendanceRequest query);
    }
}
