using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class GetAllStudentCounterScoreBySubjectTeacherResult
    {
        public string TeacherName { set; get; }
        public string ClassIdGenerated { set; get; }
        public string IdLesson { set; get; }
        public string IdSubject { set; get; }
        public string SubjectID { set; get; }
        public string IdSubjectLevel { set; get; }
        public string SubjectName { set; get; }
        public string IdGrade { set; get; }
        public string IdPeriod { set; get; }
        public string Term { set; get; }
        public DateTime? PeriodEndDate { set; get; }
        public int Semester { set; get; }
        public int TotalCounter { set; get; }
        public int TotalSubmitted { set; get; }
        public int TotalPending { set; get; }
        public int TotalUnsubmitted { set; get; }
    }
}
