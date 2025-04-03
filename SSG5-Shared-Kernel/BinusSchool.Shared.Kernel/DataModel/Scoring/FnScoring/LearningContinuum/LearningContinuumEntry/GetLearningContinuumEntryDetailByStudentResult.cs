using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumEntry
{
    public class GetLearningContinuumEntryDetailByStudentResult
    {
        public ItemValueVm NextStudent { get; set; }
        public ItemValueVm PrevStudent { get; set; }
        public DateTime? LastSavedDate { get; set; }
        public string LastSavedBy { get; set; }
        public bool IsEnrolled { get; set; }
        public List<string> IdLearningContinuumList { get; set; }
    }
}
