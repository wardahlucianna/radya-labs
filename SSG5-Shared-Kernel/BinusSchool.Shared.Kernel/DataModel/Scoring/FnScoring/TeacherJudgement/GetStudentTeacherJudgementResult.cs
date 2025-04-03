using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherJudgement
{
    public class GetStudentTeacherJudgementResult
    {
        public List<GetStudentTeacherJudgementResult_Header> Header { set; get; }
        public List<GetStudentTeacherJudgementResult_Body> Body { set; get; }
    }

    public class GetStudentTeacherJudgementResult_Header
    {
        public string IdComponent { set; get; }
        public int Semester { set; get; }
        public string ComponentShortDesc { set; get; }
        public string ComponentLongDesc { set; get; }
        public int OrderNumberComponent { set; get; }
        public List<GetStudentSubComponentVM_Header> SubComponentList { set; get; }
    }

    public class GetStudentSubComponentVM_Header
    {
        public int Semester { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string SubComponentShortDesc { set; get; }
        public string SubComponentLongDesc { set; get; }
        public decimal SubComponentWeight { get; set; }
        public string IdSubComponentCounter { set; get; }
        public string SubComponentCounterLongDesc { set; get; }
        public decimal? SubComponentCounterWeight { get; set; }
        public decimal MaxScore { get; set; }
        public int OrderNumberComponent { set; get; }
        public int OrderNumberSubComponent { set; get; }
        public DateTime? DateCounter { set; get; }
    }

    public class GetStudentTeacherJudgementResult_Body
    {
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public bool ShowCodeOption { set; get; }
        public List<GetStudentComponentVM_Body> ComponentList { set; get; }
    }

    public class GetStudentComponentVM_Body
    {
        public string IdComponent { set; get; }
        public int OrderNumberComponent { set; get; }
        public int Semester { set; get; }
        public bool IsInActiveSmt { set; get; }
        public List<GetStudentSubComponentVM_Body> SubComponentList { set; get; }
    }
    public class GetStudentSubComponentVM_Body
    {
        public string IdSubComponent { set; get; }
        public int OrderNumberSubComponent { set; get; }
        public decimal SubComponentWeight { set; get; }
        public decimal SubComponentMaxScore { set; get; }
        public GetStudentSubComponentScoreVM_Body SubComponentScoreList { set; get; }
    }

    public class GetStudentSubComponentScoreVM_Body
    {
        public decimal SubComponentScore { set; get; }
        public string TextScore { set; get; }
        public bool IsAdjusmentScore { set; get; }
        public List<GetStudentSubComponentCounterScoreVM_Body> CounterScoreList { get; set; }
    }

    public class GetStudentSubComponentCounterScoreVM_Body
    {
        public string IdSubComponentCounter { set; get; }
        public decimal? SubComponentCounterScore { set; get; }
        public decimal? SubComponentCounterWeight { set; get; }
    }

    public class GetStudentTeacherJudgementResult_SubjectSplitMapping
    {
        public string IdSubject { set; get; }
        public string IdLesson { set; get; }
    }

   
}
