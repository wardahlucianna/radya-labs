using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumSummary
{
    public class GetLearningContinuumSummaryDetailByStudentResult
    {
        public GetLearningContinuumSummaryDetailByStudentResult_Student NextStudent { get; set; }
        public GetLearningContinuumSummaryDetailByStudentResult_Student PrevStudent { get; set; }
        public DateTime? LastSavedDate { get; set; }
        public string LastSavedBy { get; set; }
        public bool IsEnrolled { get; set; }
        public List<string> IdLearningContinuumList { get; set; }
    }

    public class GetLearningContinuumSummaryDetailByStudentResult_Student : ItemValueVm
    {
        public ItemValueVm Grade { get; set; }
    }
}
