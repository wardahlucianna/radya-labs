using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetStudentCounselingResult
    {
        public string AcademicYear { get; set; }
        public string CounselorCategory { get; set; }
        public string CounselorName { get; set; }
        public DateTime CounselorDate { get; set; }
    }
}
