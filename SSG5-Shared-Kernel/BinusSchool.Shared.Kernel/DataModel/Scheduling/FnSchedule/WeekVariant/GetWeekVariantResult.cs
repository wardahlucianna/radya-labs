using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.WeekVariant
{
    public class GetWeekVariantResult : CodeWithIdVm
    {
        public IEnumerable<CodeWithIdVm> Weeks { get; set; }
    }
}