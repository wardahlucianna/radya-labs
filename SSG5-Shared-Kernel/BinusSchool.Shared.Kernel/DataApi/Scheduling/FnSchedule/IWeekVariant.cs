using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.WeekVariant;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IWeekVariant : IFnSchedule
    {
        [Get("/schedule/week-variant")]
        Task<ApiErrorResult<IEnumerable<GetWeekVariantResult>>> GetWeekVariants(CollectionSchoolRequest query);
    }
}
