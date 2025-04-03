using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface ICheckUsage : IFnSchedule
    {
        [Get("/schedule/check-usage/pathway-detail")]
        Task<ApiErrorResult<IDictionary<string, bool>>> CheckUsagePathwayDetails(IdCollection ids);
    }
}