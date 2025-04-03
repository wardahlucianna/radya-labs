using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.BrailleExpLevel;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IBrailleExpLevel : IFnStaff
    {
        [Get("/staff/braille-exp-level")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetBrailleExpLevels(CollectionRequest query);
    }
}
