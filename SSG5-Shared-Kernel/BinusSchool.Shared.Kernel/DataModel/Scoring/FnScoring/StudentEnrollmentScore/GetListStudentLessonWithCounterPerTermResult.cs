using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class GetListStudentLessonWithCounterPerTermResult
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdPeriod { set; get; }
        public string Term { set; get; }
        public DateTime? PeriodEndDate { set; get; }
        public int Semester { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string ClassIdGenerated { set; get; }
        public string IdTeacher { set; get; }
        public string TeacherName { set; get; }
        public string IdCounter { set; get; }
        public string IdSubject { set; get; }
        public string SubjectID { set; get; }
        public string SubjectName { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
        public bool HideScoreSummary { set; get; }
        public string? IdSubComponent { get; set; }
        public bool? EnableTeacherJudgement { get; set; }
        public bool? EnablePredictedGrade { get; set; }
    }
    public class GetListStudentLessonWithCounterPerTermResult_SubjectCounter
    {
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdCounter { set; get; }
        public string IdPeriod { set; get; }
        public string Term { set; get; }
        public DateTime? PeriodEndDate { set; get; }
        public int Semester { set; get; }
    }

    public class GetListStudentLessonWithCounterPerTermResult_Counter
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdPeriod { set; get; }
        public string Term { set; get; }
        public DateTime? PeriodEndDate { set; get; }
        public int Semester { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string ClassIdGenerated { set; get; }
        public string IdTeacher { set; get; }
        public string TeacherName { set; get; }
        public List<GetListStudentLessonWithCounterPerTermResult_SubjectCounter> Counter { set; get; }
        public string IdSubject { set; get; }
        public string SubjectID { set; get; }
        public string SubjectName { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
        public bool HideScoreSummary { set; get; }
    }
}
