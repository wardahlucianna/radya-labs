using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentAcademicScore
{
    public class GetStudentAcademicScoreForMobileResult
    {
        public ItemValueVm Subject { set; get; }
        public ItemValueVm SubjectType { set; get; }
        public ItemValueVm SubjectLevel { set; get; }
        public List<NameValueVm> Teacher { set; get; }
        public List<GetStudentAcademicScoreForMobileResult_SmtScore> SmtScore { set; get; }
        public GetStudentAcademicScoreForMobileResult_Final FinalScore { set; get; }
        public GetStudentAcademicScoreForMobileResult_Final FinalGrade { set; get; }
        public decimal? MaxScore { set; get; }
        public bool? ShowGradingAsScore { set; get; }
    }

    public class GetStudentAcademicScoreForMobileResult_SmtScore
    {
        public string IdSmt { set; get; }
        public string SmtDesc { set; get; }
        public string Score { set; get; }
        public string Grade { set; get; }
        public List<GetStudentAcademicScoreForMobileResult_TermScore> TermScore { set; get; }
    }

    public class GetStudentAcademicScoreForMobileResult_TermScore
    {
        public string IdTerm { set; get; }
        public string TermDesc { set; get; }
        public string Score { set; get; }
        public string Grade { set; get; }
        public List<GetStudentAcademicScoreForMobileResult_ComponentScore> ComponentScore { set; get; }
    }

    public class GetStudentAcademicScoreForMobileResult_ComponentScore
    {
        public int? OrderNo { set; get; }
        public string IdComponent { set; get; }
        public string ComponentDesc { set; get; }
        public string ComponentShortDesc { set; get; }
        public string Score { set; get; }
        public string Grade { set; get; }
        public List<GetStudentAcademicScoreForMobileResult_SubComponentScore> SubComponentScore { set; get; }
        public bool ShowGradingAsScore { set; get; }
    }

    public class GetStudentAcademicScoreForMobileResult_SubComponentScore
    {
        public int? OrderNo { set; get; }
        public string IdSubComponent { set; get; }
        public string SubComponentDesc { set; get; }
        public string SubComponentShortDesc { set; get; }
        public string Score { set; get; }
        public string Grade { set; get; }
        public List<GetStudentAcademicScoreForMobileResult_SubComponentCounterScore> SubComponentCounterScore { set; get; }
    }

    public class GetStudentAcademicScoreForMobileResult_SubComponentCounterScore
    {
        public string IdSubComponentCounter { set; get; }
        public string SubComponentCounterDesc { set; get; }
        public string SubComponentCounterShortDesc { set; get; }
        public string Score { set; get; }
        public string Grade { set; get; }
    }

    public class GetStudentAcademicScoreForMobileResult_Final
    {
        public string FinalDesc { set; get; }
        public string Score { set; get; }
    }

    public class GetStudentAcademicScoreForMobileResult_CounterDetail
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdCounter { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
    }

    public class GetStudentAcademicScoreForMobileResult_Counter
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public List<GetStudentAcademicScoreForMobileResult_SubjectCounter> Counter { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
    }

    public class GetStudentAcademicScoreForMobileResult_trscore
    {
        public string IdStudent { set; get; }
        public string IdCounter { set; get; }
        public decimal Score { set; get; }
    }

    public class GetStudentAcademicScoreForMobileResult_SubjectCounter
    {
        public string IdSubject { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdCounter { set; get; }
    }
}
