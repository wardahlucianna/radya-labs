using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentLearningContinuumResult
    {
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdSubject { get; set; }
        public string SubjectDescription { get; set; }
        public int LocAchieved { get; set; }
        public int LocNotAchieved { get; set; }
        public List<GetComponentLearningContinuumResult_LocList> LocList { get; set; }
    }

    public class GetComponentLearningContinuumResult_LocList
    {
        public ItemValueVm LearningContinuumType { get; set; }
        public int LocAchieved { get; set; }
        public int LocTotalItems { get; set; }
        public List<GetComponentLearningContinuumResult_LocList_LocCategory> LearningContinuumCategories { get; set; }
    }

    public class GetComponentLearningContinuumResult_LocList_LocCategory : ItemValueVm
    {
        public List<GetComponentLearningContinuumResult_LocList_LocCategory_Phase> Phases { get; set; }
    }

    public class GetComponentLearningContinuumResult_LocList_LocCategory_Phase
    {
        public int Phase { get; set; }
        public List<GetComponentLearningContinuumResult_LocList_LocCategory_Phase_LocItem> LocItems { get; set; }
    }

    public class GetComponentLearningContinuumResult_LocList_LocCategory_Phase_LocItem : ItemValueVm
    {
        public string IdLearningContinuum { get; set; }
        public bool IsChecked { get; set; }
    }
}
