using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.EmployeeStatus;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IEmployeeStatus : IFnStaff
    {
        [Get("/staff/employee-status")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetEmployeeStatus(CollectionRequest query);
    }
}