using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.IMTASchoolLevel;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IIMTASchoolLevel : IFnStaff
    {
        [Get("/staff/imta-school-level")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetIMTASchoolLevels(CollectionRequest query);
    }
}
