using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum
{
    public class GetLearningContinuumResult
    {
        public List<GetLearningContinuumResult_Subject> Subject { get; set; }
        public List<GetLearningContinuumResult_Class> Class { get; set; }
    }

    public class GetLearningContinuumResult_Subject
    {
        public string ClassIdGenerated { get; set; }
        public string SubjectID { get; set; }
        public ItemValueVm Subject { get; set; }
        public ItemValueVm Grade { get; set; }
    }

    public class GetLearningContinuumResult_Class
    {
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Grade { get; set; }
    }
}
