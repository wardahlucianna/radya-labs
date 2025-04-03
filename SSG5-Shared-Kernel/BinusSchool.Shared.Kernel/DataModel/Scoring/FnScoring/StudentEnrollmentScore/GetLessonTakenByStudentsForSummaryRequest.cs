using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class GetLessonTakenByStudentsForSummaryRequest
    {
        public string IdUser { set; get; }
        public string IdSchool { set; get; }
        public string IdAcademicYear { set; get; }
        public int Semester { set; get; }
        public string? IdLevel { set; get; }
        public string? IdGrade { set; get; }
        public string? IdHomeroom { set; get; }
        public string? IdSubject { set; get; }
        public string? IdDepartment { set; get; }
        public string? IdStudent { get; set; }
        public bool? HideMappingSubject { set; get; }
    }
}
