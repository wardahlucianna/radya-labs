using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselorData
{
    public class GetLevelGradeByConcellorRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdUser { get; set; }
        public string IdRole { get; set; }
        public string IdPosition { get; set; }
    }
}
