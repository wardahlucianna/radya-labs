using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentAffectiveScore
{
    public class GetStudentSubjectAffectiveScoreDetailResult
    {
        public string finalScore { set; get; }
        public string ScoreColor { set; get; }
        public string finalGrade { set; get; }
        public string predictedGrade { set; get; }
        public List<StudentSubjectAffectiveScore_SmtVm> semesterList { set; get; }


    }

    public class StudentSubjectAffectiveScore_SmtVm
    {
        public int semester { set; get; }
        public List<StudentSubjectAffectiveScore_ComponentVm> detailComponentList { set; get; }

    }
    public class StudentSubjectAffectiveScore_ComponentVm
    {
        public string componentName { set; get; }
        public string sourceSubject { set; get; }
        public string sourceComponent { set; get; }
        public string score { set; get; }
    }

}
