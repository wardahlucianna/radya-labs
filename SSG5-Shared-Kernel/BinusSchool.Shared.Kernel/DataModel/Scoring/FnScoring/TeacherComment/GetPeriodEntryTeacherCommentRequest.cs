using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment
{
    public class GetPeriodEntryTeacherCommentRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdGrade { set; get; }
        public string IdPeriod { get; set; }
    }
}
