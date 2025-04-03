using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.EducationLevel;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IEducationLevel : IFnStaff
    {
        [Get("/staff/education-level")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetEducationLevels(CollectionRequest query);
    }
}