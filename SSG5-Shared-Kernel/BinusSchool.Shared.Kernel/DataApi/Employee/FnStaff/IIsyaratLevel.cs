using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.IsyaratLevel;
using Refit;
namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IIsyaratLevel : IFnStaff
    {
        [Get("/staff/isyarat-level")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetIsyaratLevels(CollectionRequest query);
    }
}
