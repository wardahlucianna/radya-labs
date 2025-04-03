using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.StaffInfoUpdate;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IStaffInfoUpdate : IFnStaff
    {
        //[Get("/employee/student-info-update")]
        //Task<ApiErrorResult<IEnumerable<GetStaffInfoUpdateResult>>> GetStaffInfoUpdates(GetStaffInfoUpdateRequest query);

        //[Put("/employee/update-staff-certification")]
        //Task<ApiErrorResult> UpdateStaffCertification([Body] GetStaffInfoUpdateRequest[] body);

        //[Put("/employee/update-staff-education")]
        //Task<ApiErrorResult> UpdateStaffEducation([Body] GetStaffInfoUpdateRequest[] body);

        //[Put("/employee/update-staff-job-info")]
        //Task<ApiErrorResult> UpdateStaffJobInfo([Body] GetStaffInfoUpdateRequest[] body);
    }
}
