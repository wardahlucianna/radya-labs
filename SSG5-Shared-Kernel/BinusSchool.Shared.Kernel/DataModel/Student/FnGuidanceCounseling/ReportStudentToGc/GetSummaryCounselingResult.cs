using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetSummaryCounselingResult
    {
        public string IdStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public string AcademicYear { get; set; }
        public string StudentName { get; set; }
        public string BinusanID { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public int Semester { get; set; }
        public bool IsCounsellor { get; set; }
    }
}
