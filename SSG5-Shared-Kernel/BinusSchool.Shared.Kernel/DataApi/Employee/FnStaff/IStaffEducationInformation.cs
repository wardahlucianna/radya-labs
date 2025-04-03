using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;
using BinusSchool.Data.Model.Employee.FnStaff.StaffEducationInformation;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IStaffEducationInformation : IFnStaff
    {
        [Get("/staff/staff-education-information/{id}")]
        Task<ApiErrorResult<IEnumerable<GetStaffEducationInformationResult>>> GetStaffEducationInformations(string id);
    }
}
