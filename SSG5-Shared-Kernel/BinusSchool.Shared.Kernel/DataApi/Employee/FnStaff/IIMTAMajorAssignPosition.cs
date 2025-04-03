using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.IMTAMajorAssignPosition;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IIMTAMajorAssignPosition : IFnStaff
    {
        [Get("/staff/imta-major-assign-position")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetIMTAMajorAssignPositions(CollectionRequest query);
    }
}