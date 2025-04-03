using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByTeacher
{
    public class GetTeacherParameterDescriptionResult
    {
        public string School { set; get; }
        public string AcademicYear { set; get; }
        public int Semester { set; get; }
        public string Level { set; get; }
        public string Grade { set; get; }
        public string Teacher { get; set; }
        public string Department { get; set; }
        public string Subject { get; set; }
        public string SubjectType { get; set; }
    }
}
