using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumEntry
{
    public class SaveLearningContinuumEntryByStudentRequest
    {
        public string IdStudent { get; set; }
        public string IdGrade { get; set; }
        public string IdSubjectContinuum { get; set; }
        public List<string> IdLearningContinuumList { get; set; }
    }
}
