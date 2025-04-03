using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.LabSkillsLevel;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface ILabSkillsLevel : IFnStaff
    {
        [Get("/staff/lab-skills-level")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetLabSkillsLevels(CollectionRequest query);
    }
}