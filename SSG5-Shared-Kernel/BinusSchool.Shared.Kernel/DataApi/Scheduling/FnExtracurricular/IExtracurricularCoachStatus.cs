using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IExtracurricularCoachStatus : IFnExtracurricular
    {
        [Get("/elective/coach-status")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> ExtracurricularCoachStatus();

    }
}
