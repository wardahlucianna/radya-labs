using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByTeacher
{
    public class GetTeacherListRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdSchool { get; set; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
    }
}
