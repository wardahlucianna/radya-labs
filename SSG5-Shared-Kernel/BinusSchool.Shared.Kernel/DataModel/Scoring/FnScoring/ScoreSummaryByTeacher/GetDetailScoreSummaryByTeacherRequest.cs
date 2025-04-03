using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByTeacher
{
    public class GetDetailScoreSummaryByTeacherRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdDepartment { get; set; }
        public string IdSubject { get; set; }
        public string IdUser { set; get; }
    }
}
