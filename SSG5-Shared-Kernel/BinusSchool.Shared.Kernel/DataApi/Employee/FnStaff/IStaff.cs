using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.Employee.FnStaff.Staff;
using BinusSchool.Data.Model;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IStaff : IFnStaff
    {
        [Post("/employee/teacher/get-teacher-for-asc")]
        Task<ApiErrorResult<IEnumerable<CheckTeacherForAscTimetableResult>>> GetTeacherFoUploadAsc([Body] CheckTeacherForAscTimetableRequest body);

        [Get("/staff/unmap-to-user")]
        Task<ApiErrorResult<IEnumerable<GetUnmapStaffResult>>> GetUnmapStaffs(CollectionSchoolRequest query);

        [Get("/staff/staff-by-school")]
        Task<ApiErrorResult<IEnumerable<GetStaffBySchoolResult>>> GetStaffBySchool(CollectionSchoolRequest query);
    }
}
