using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesObjective;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IElectivesObjective : IFnExtracurricular
    {
        [Delete("/elective/elective-objective")]
        Task<ApiErrorResult> DeleteElectivesObjective([Body] IEnumerable<string> ids);

        [Get("/elective/elective-objective")]
        Task<ApiErrorResult<IEnumerable<GetElectivesObjectiveResult>>> GetElectivesObjective(GetElectivesObjectiveRequest body);

        [Post("/elective/elective-objective")]
        Task<ApiErrorResult> AddElectivesObjective([Body] AddElectivesObjectiveRequest body);
    }
}
