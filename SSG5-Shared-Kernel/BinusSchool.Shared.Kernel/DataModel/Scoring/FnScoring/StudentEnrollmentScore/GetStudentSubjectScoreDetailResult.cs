using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentEnrollmentScore
{
    public class GetStudentSubjectScoreDetailResult
    {
        public string Description { set; get; }
        public List<SubjectScore_ComponentVm> ComponentList { set; get; }
    }

    public class SubjectScore_ComponentVm
    {
        public string IdComponent { set; get; }
        public string ComponentName { set; get; }
        public List<SubjectScore_ComponentHeaderVm> HeaderList { set; get; }
        public List<SubjectScore_SubComponentVm> SubComponentList { set; get; }
        public decimal? TotalWeight { set; get; }
        public decimal? TotalScore { set; get; }
        public string Overall { set; get; }

    }
    public class SubjectScore_ComponentHeaderVm
    {
        public string Counter { set; get; }
        public string CounterName { set; get; }

    }
    public class SubjectScore_SubComponentVm
    {
        public string SubComponentName { set; get; }
        public string SubComponentScore { set; get; } //total
        public decimal? SubComponentWeighted { set; get; } //%

        public List<SubjectScore_SubComponentCounterVm> ComponentCounterList { set; get; }

    }

    public class SubjectScore_SubComponentCounterVm
    {
        public string IdSubComponentCounter { set; get; }
        public string Counter { set; get; }
        public decimal? Score { set; get; }
        public string CounterScore { set; get; }

    }

    public class SubjectScoreAdditionalCodeVm
    {
        public decimal Score { set; get; }
        public string IdSchool { set; get; }
        public string LongDesc { set; get; }
        public bool ShowScoreAsNA { set; get; }
        public string CategoryScore { set; get; }

    }

    public class StudentSubjectScoreDetaiResultVm
    {
        public string IdSchool { set; get; }
        public decimal? TotalScore { set; get; }
        public string Grade { set; get; }      
        public string IdLesson { set; get; }
        public string IdSubjectScoreSetting { set; get; }
        public decimal TotalWeight { set; get; }

        public string IdComponent { set; get; }
        public string ComponentShortDesc { set; get; }
        public string ComponentLongDesc { set; get; }
        public decimal? ComponentScore { set; get; }
        public int OrderNumberComponent { set; get; }
        

        public string IdSubComponent { set; get; }
        public string SubComponentShortDesc { set; get; }
        public string SubComponentLongDesc { set; get; }
        public decimal SubComponentWeight { set; get; }
        public decimal SubComponentScore { set; get; }
        public decimal? SubComponentWeightedAverage { set; get; }
        public int OrderNumberSubComponent { set; get; }

        public string IdSubComponentCounter { set; get; }
        public string SubComponentCounterShortDesc { set; get; }
        public string SubComponentCounterLongDesc { set; get; }
        public decimal? SubComponentCounterScore { set; get; }
        public decimal Score { set; get; }
        public string AdditionalScoreName { set; get; }
        public bool ShowScoreAsNA { set; get; }
        public string Category { set; get; }      


    }


    public class SubjectComponentWeightScoreVm
    {
        public string IdSubjectScoreSetting { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdComponent { set; get; }
        public decimal TotalWeight { set; get; }

    }
}
