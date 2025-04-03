using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.GeneralScoreEntryPeriod
{
    public class GetAllSubjectsResult
    {
        public ItemValueVm Level { get; set; }
        public List<GetAllSubjectsResult_Grades> Grades { get; set; }

        #region ndak jadi
        /*public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public string IdSubjectScoreFinalSetting { get; set; }
        public ItemValueVm Subject { get; set; }
        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }*/
        #endregion
    }

    public class GetAllSubjectsResult_Grades
    {
        public ItemValueVm Grade { get; set; }
        public List<GetAllSubjectsResult_Subjects> Subjects { get; set; }
    }

    public class GetAllSubjectsResult_Subjects
    {
        public string IdSubjectScoreFinalSetting { get; set; }
        public ItemValueVm Subject { get; set; }
        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        public ItemValueVm SubjectLevel { get; set; }
        public string SubjectId { get; set; }
    }
}
