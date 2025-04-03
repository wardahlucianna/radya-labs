using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;
using BinusSchool.Data.Model.Employee.FnStaff.StaffJobInformation;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IStaffJobInformation : IFnStaff
    {
        [Put("/employee/staff-job-information")]
        Task<ApiErrorResult> UpdateStaffJobInformation([Body] UpdateStaffJobInformationRequest body);
    }
}
