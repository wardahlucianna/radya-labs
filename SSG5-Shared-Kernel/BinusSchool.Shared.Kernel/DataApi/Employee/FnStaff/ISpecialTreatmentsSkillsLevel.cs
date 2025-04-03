using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.SpecialTreatmentsSkillsLevel;
using Refit;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface ISpecialTreatmentsSkillsLevel : IFnStaff
    {
        [Get("/staff/special-treatments-skills-level")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetSpecialTreatmentsSkillsLevels(CollectionRequest query);
    }
}