using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class ExportExcelStudentScoreForSummaryRequest
    {
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string IdSchool { set; get; }
        public string? IdLevel { set; get; }
        public string? IdGrade { set; get; }
        public string? IdHomeroom { set; get; }
        public string? IdSubject { set; get; }
        public string? IdDepartment { set; get; }
        public string? IdStudent { get; set; }
        public bool ShowAllCounter { set; get; }
        public bool ShowSubmitted { set; get; }
        public bool ShowPending { set; get; }
        public bool ShowUnsubmitted { set; get; }
    }
}
