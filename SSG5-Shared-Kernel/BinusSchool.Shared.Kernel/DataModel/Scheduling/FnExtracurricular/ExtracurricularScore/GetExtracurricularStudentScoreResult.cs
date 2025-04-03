using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class GetExtracurricularStudentScoreResult
    {     
      public List<ExtracurricularComponentScore_HeaderVm> Header { get; set; }
      public List<ExtracurricularComponentScore_BodyVm> Body { get; set; }
      public string CalculationType { get; set; }
    }

    public class ExtracurricularComponentScore_HeaderVm 
    {
        public string idScoreComponent { get; set; }
        public string ScoreComponentName { get; set; }
    }

    public class ExtracurricularComponentScore_BodyVm
    {
        public string ExtracurricularName { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string Homeroom { get; set; }
        public decimal IntScore { get; set; }
        public string FinalScore { get; set; }
        public string Grade { get; set; }
        public List<ExtracurricularComponentScoreVm> ComponentScores { get; set; }
    }


    public class ExtracurricularComponentScoreVm
    {
        public string IdExtracurricularScoreEntry { get; set; }
        public string IdScoreComponent { get; set; }
        public string ScoreComponentName { get; set; }
        public ItemValueVm score { get; set; }
        public string LastestAudit { get; set; }
        public string AuditActivity { get; set; }
    }

    public class GetExtracurricularStudentScoreVm
    {
        public string ExtracurricularName { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public int OrderNumberComponent { get; set; }
        public string idScoreComponent { get; set; }
        public string ScoreComponentName { get; set; }
        public string IdExtracurricularScoreEntry { get; set; }
        public ItemValueVm score { get; set; }
        public string LastestAudit { get; set; }
        public string AuditActivity { get; set; }

    }


}
