using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherJudgement
{
    public class UpdateStudentTeacherJudgementRequest
    {
        public string IdPeriod { get; set; }
        public string IdSubject { get; set; }
        public string IdSubjectLevel { get; set; }
        //public bool EnableTeacherJudgement { get; set; }
        //public ApprovalStatus StatusApproval { get; set; }
        public List<TeacherJudgementStudentList> TeacherJudgementStudentList { get; set; }
    }
    public class TeacherJudgementStudentList
    {    
        public string IdStudent { get; set; }
        public List<StudentSubComponentScoreList> StudentSubComponentScoreList { get; set; }
    }
    public class StudentSubComponentScoreList
    {
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        //public string IdSubComponentCounter { set; get; }
        public string TextScore { set; get; }
        public decimal? Score { get; set; }
        //public bool IsAdjusmentScore { get; set; }
    }

    public class RekalkulasiStudentScoreTeacherJudgementVm
    {
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdSubjectScoreSetting { set; get; }
        public decimal Weight { set; get; }
        public decimal Score { set; get; }
        public bool IsAvg { set; get; }
    }

    public class TeacherJudgement_SubjectMappingVm
    {
        public string IdStudent { get; set; }
        public string IdSubjectScoreSetting { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
      
    }
}
