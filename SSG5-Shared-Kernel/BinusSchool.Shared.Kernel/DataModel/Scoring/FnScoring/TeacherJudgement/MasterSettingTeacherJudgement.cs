using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherJudgement
{
    public class MasterSettingTeacherJudgement
    {
        
    }
    
    public class ComponentSettingVM
    {
        public string IdComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public List<SubComponentSettingVM> SubComponentList { set; get; }
    }
    //public class ComponentSettingScoreVM
    //{
    //    public string IdComponent { set; get; }
    //    public string ShortDesc { set; get; }
    //    public string LongDesc { set; get; }
    //    public List<SubComponentSettingVM> StudentSubComponentScoreList { set; get; }
    //}
    public class SubComponentSettingVM
    {
        public string IdSubComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public decimal Weight { set; get; }
        public bool AverageCounterScore { set; get; }
        public List<StudentSubComponentScoreVm> StudentSubComponentScoreList { set; get; }
    }
    public class StudentSubComponentScoreVm
    {
        //public string IdSubComponent { set; get; }
        //public string ShortDesc { set; get; }
        //public string LongDesc { set; get; }
        public decimal Score { set; get; }
        public bool IsAdjusmentScore { set; get; }
    }
    //public class ScoreOptionSettingVM
    //{
    //    public string IdSchool { get; set; }
    //    public string ShortDesc { get; set; }
    //    public string LongDesc { get; set; }
    //    public string InputType { get; set; }
    //    public decimal MinScoreLength { get; set; }
    //    public decimal MaxScoreLength { get; set; }
    //    public bool CurrentStatus { get; set; }
    //}
}
