using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScorePredictedGrade
{
    public class GetStudentScorePredictedGradeResult
    {
        public List<StudentScorePredictedGradeHeader_ComponentSemsterVm> header { set; get; }
        public List<StudentScorePredictedGrade_SubjectVm> body { set; get; }
    }

    public class StudentScorePredictedGradeHeader_ComponentSemsterVm
    {
        public int Semester { set; get; }
        public List<StudentScorePredictedGradeHeader_ComponentVm> ComponentList { set; get; }
    }

    public class StudentScorePredictedGradeHeader_ComponentVm
    {
        public string IdComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
    }

    public class StudentScorePredictedGrade_SubjectVm
    {
        public string IdSubjectScoreSetting { get; set; }
        public string IdSubjectScore { get; set; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public decimal TotalScore { set; get; }
        public string ScoreColor { set; get; }
        public string Grade { set; get; }
        public string PredictedGrade { set; get; }
        public List<StudentScorePredictedGrade_ComponentSemsterVm> SemesterList { set; get; }
    }

    public class StudentScorePredictedGrade_ComponentSemsterVm
    {
        public int Semester { set; get; }
        public List<StudentScorePredictedGrade_ComponentVm> ComponentList { set; get; }
    }

    public class StudentScorePredictedGrade_ComponentVm
    {
        public string IdComponent { set; get; }
        public string ShortDesc { set; get; }
        public string ComponentScore { set; get; }

    }

    public class StudentScorePredictedGrade_SubjectSetting
    {
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdSubjectScoreSetting { get; set; }
        public string IdPeriod { set; get; }
        public decimal MaxScore { set; get; }
        public decimal MinScore { set; get; }
    }

    public class StudentScorePredictedGrade_StudentSubjectSetting
    {
        public string IdStudent { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        public string IdSubjectScoreSetting { get; set; }
        public string IdPeriod { set; get; }
        public decimal MaxScore { set; get; }
        public decimal MinScore { set; get; }
    }

    public class StudentScorePredictedGrade_SubjectScore
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string IdSubjectScoreSetting { get; set; }
        public string? IdSubjectScore { get; set; }
        public string? IdSubjectScorePredictedGrade { get; set; }
        public decimal? Score { set; get; }
        public string? Grade { set; get; }
        public string? PredictedGrade { set; get; }
        public decimal MaxScore { set; get; }
        public decimal MinScore { set; get; }
    }

    public class StudentScorePredictedGrade_StudentComponentScore
    {
        public string IdStudent { get; set; }
        public string IdSubjectScoreSetting { get; set; }
        public string IdComponent { get; set; }
        public string ComponentShortDesc { get; set; }
        public string ComponentLongDesc { get; set; }
        public int ComponentOrderNo { get; set; }
        public decimal? Score { set; get; }
        public string? AdditionalScoreName { set; get; }
        public bool? ShowScoreAsNA { set; get; }
    }
}
