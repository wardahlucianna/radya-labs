using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class GetLessonTakenByStudentsForSummaryResult
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string ClassIdGenerated { set; get; }
        public string IdSubject { set; get; }
        public string SubjectID { set; get; }
        public string SubjectName { set; get; }
        public string IdSubjectType { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
        public bool HideScoreSummary { set; get; }
    }

    public class GetLessonTakenByStudentsForSummaryResult_SubjectCounter
    {
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdCounter { set; get; }
    }

    public class GetLessonTakenByStudentsForSummaryResult_WithCounterDetail
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string ClassIdGenerated { set; get; }
        public string IdTeacher { set; get; }
        public string TeacherName { set; get; }
        public string IdCounter { set; get; }
        public string IdSubject { set; get; }
        public string SubjectID { set; get; }
        public string SubjectName { set; get; }
        public string IdSubjectType { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
        public bool HideScoreSummary { set; get; }
    }

    public class GetLessonTakenByStudentsForSummaryResult_WithCounter
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string ClassIdGenerated { set; get; }
        public string IdTeacher { set; get; }
        public string TeacherName { set; get; }
        public List<string> Counter { set; get; }
        public string IdSubject { set; get; }
        public string SubjectID { set; get; }
        public string SubjectName { set; get; }
        public string IdSubjectType { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
        public bool HideScoreSummary { set; get; }
    }
}
