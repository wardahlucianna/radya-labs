using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByDept
{
    public class GetDetailScoreSummaryByDeptRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdSchool { get; set; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdDepartment { set; get; }
    }
}
