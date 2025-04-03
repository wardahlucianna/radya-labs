using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class GetAllStudentCounterScoreForSummaryResult
    {
        public string IdStudent { set; get; }
        public string IdLesson { set; get; }
        public string IdCounter { set; get; }
        public string IdSubComponent { get; set; }
        public string IdTeacher { set; get; }
        public string TeacherName { set; get; }
        public string? Score { set; get; }
        public string? Category { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdSubject { set; get; }
        public string IdDepartment { set; get; }
    }
}
