using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByDept
{
    public class GetStudentScoreViewByDeptRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdDept { set; get; }
        public string IdSubjectType { set; get; }
        public string IdSubject { set; get; }
        public string IdTeacher { set; get; }
    }
}
