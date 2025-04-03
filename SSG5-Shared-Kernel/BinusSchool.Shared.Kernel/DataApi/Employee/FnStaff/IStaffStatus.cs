using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.StaffStatus;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IStaffStatus : IFnStaff
    {
        [Get("/staff/staff-status")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetStaffStatus(CollectionRequest query);
    }
}