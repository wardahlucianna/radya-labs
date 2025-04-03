using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.PTKType;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IPTKType : IFnStaff
    {
        [Get("/staff/ptk-type")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetPTKTypes(CollectionRequest query);
    }
}