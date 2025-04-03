using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.GeneralScoreEntryPeriod
{
    public class GetGeneralScoreEntryPeriodListResult : CodeWithIdVm
    {
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Semester { get; set; }
        public ItemValueVm Term { get; set; }
        public ItemValueVm Subject { get; set; }
        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        public DateTime? UpdatePeriodStartDate { get; set; }
        public DateTime? UpdatePeriodEndDate { get; set; }
    }
}
