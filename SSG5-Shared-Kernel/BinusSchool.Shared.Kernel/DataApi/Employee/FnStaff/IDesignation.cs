using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.Designation;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IDesignation : IFnStaff
    {
        [Get("/staff/designation")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetDesignations(CollectionRequest query);
    }
}
