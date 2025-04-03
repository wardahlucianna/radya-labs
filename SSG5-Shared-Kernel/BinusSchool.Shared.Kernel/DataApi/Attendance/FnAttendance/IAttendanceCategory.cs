using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.Category;
using Refit;

namespace BinusSchool.Data.Api.Attendance.FnAttendance
{
    public interface IAttendanceCategory : IFnAttendance
    {
        [Get("/attendance-category")]
        Task<ApiErrorResult<GetAttendanceCategoryResult>> GetAttendanceCategories(GetAttendanceCategoryRequest param);
    }
}
