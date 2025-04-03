using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum
{
    public class GetListLearningContinuumHistoryResult
    {
        public string IdHistoryLearningContinuum { get; set; }
        public DateTime? SavedDate { get; set; }
        public string SavedBy { get; set; }
        public List<string>? IdLearningContinuumList { get; set; }
    }

    public class learningContinuumVm
    {
        public string IdLearningContinuum { get; set; }
    }
}
