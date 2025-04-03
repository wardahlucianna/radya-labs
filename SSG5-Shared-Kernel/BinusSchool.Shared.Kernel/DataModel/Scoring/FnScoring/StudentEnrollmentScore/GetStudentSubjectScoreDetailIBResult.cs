using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class GetStudentSubjectScoreDetailIBResult
    {
        public ItemValueVm Term { set; get; }
        public List<SubjectScore_HeaderIBVm> header { set; get; }
        public List<SubjectScore_BodyIBVm> body { set; get; }
        public SubjectScore_SummaryIBVm summary { set; get; }
    }

    public class SubjectScore_HeaderIBVm
    {
        public string headerId { set; get; }
        public string description { set; get; }
    }

    public class SubjectScore_BodyIBVm
    {
        public string dateOfAssessment { set; get; }
        public string description { set; get; }
        public string assessmentName { set; get; }      
        public List<SubjectScore_ScoreIBVm> scoreList { set; get; }
    }

    public class SubjectScore_SummaryIBVm
    {
        public string studentTotal { set; get; }
        public string maxScore { set; get; }
        public string totalDivide7 { set; get; }
        public List<SubjectScore_ScoreSummaryIBVm> scoreList { set; get; }
    }

    public class SubjectScore_ScoreIBVm
    {
        public string headerId { set; get; }
        public string score { set; get; }
    }

    public class SubjectScore_ScoreSummaryIBVm
    {
        public string headerId { set; get; }
        public string description { set; get; }
        public string longDescription { set; get; }
        public string score { set; get; }
        public string maxScore { set; get; }
    }

    public class GetStudentSubjectScoreDetailIBResult_SubjectCounter
    {
        public string IdPeriod { set; get; }
        public string Term { set; get; }
        public string IdSubject { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdCounter { set; get; }
        public DateTime? DateCounter { set; get; }
    }

    public class GetStudentSubjectScoreDetailIBResult_CounterDetail
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdPeriod { set; get; }
        public string Term { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdCounter { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
        public DateTime? DateCounter { set; get; }
    }

    public class GetStudentSubjectScoreDetailIBResult_Counter
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public List<GetStudentSubjectScoreDetailIBResult_SubjectCounter> Counter { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
    }

}
