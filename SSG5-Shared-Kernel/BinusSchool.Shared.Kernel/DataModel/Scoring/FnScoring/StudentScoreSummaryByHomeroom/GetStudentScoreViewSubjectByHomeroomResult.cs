using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByHomeroom
{
    public class GetStudentScoreViewSubjectByHomeroomResult
    {
        public List<ScoreViewSubjectSubjectLevelVm> SubjectLevelStudentScoreList { set; get; }
    }

    public class ScoreViewSubjectSubjectLevelVm
    {
        public string SubjectLevel { set; get; }
        public List<ScoreViewSubjectHeader_ComponentVm> header { set; get; }
        public List<ScoreViewSubjectStudentScoreVm> body { set; get; }


    }

    public class ScoreViewSubjectStudentScoreVm
    {
        public string Homeroom { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public decimal? TotalScore { set; get; }
        public string ScoreColor { set; get; }
        public string Grade { set; get; }
        public string ScoreView { get; set; }
        public bool ShowGradingAsScore { get; set; }
        public List<ScoreViewSubject_ComponentVm> ComponentList { set; get; }

    }

    public class ScoreViewSubject_ComponentVm
    {
        public string IdComponent { set; get; }
        public List<ScoreViewSubject_SubComponentVm> SubComponentScoreList { set; get; }

    }
    public class ScoreViewSubject_SubComponentVm
    {
        public string IdSubComponent { set; get; }
        public decimal? SubComponentScore { set; get; }
        public string SubComponentScoreView { set; get; }
        public List<ScoreViewSubject_SubComponentCounterVm> CounterScoreList { set; get; }
        public decimal? weightedAverage { set; get; }

    }

    public class ScoreViewSubject_SubComponentCounterVm
    {
        public string IdSubComponentCounter { set; get; }
        public string SubComponentCounterScore  { set; get; }
        public string SubComponentCounterScoreView { set; get; }


    }

    public class ScoreViewSubjectHeader_ComponentVm
    {
        public string IdComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public List<ScoreViewSubjectHeader_SubComponentVm> SubComponentList { set; get; }
    }
    public class ScoreViewSubjectHeader_SubComponentVm
    {
        public string IdSubComponent { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
        public decimal SubComponentWeight { set; get; }
        public List<ScoreViewSubjectHeader_SubComponentCounterVm> CounterScoreList { set; get; }
    }

    public class ScoreViewSubjectHeader_SubComponentCounterVm
    {
        public string IdCounter { set; get; }
        public string ShortDesc { set; get; }
        public string LongDesc { set; get; }
    }

    public class StudentScoreViewSubjectByHomeroomResultVm
    {
        public string IdSchool { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public decimal? TotalScore { set; get; }  
        public string Grade { set; get; }
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdLesson { set; get; }
        public string IdSubjectScoreSetting { set; get; }
        public decimal TotalWeight { set; get; }

        public string IdComponent { set; get; }
        public string ComponentShortDesc { set; get; }
        public string ComponentLongDesc { set; get; }

        public string IdSubComponent { set; get; }
        public string SubComponentShortDesc { set; get; }
        public string SubComponentLongDesc { set; get; }
        public decimal SubComponentWeight { set; get; }
        public decimal SubComponentScore { set; get; }       
        public decimal SubComponentWeightedAverage { set; get; }

        public string IdSubComponentCounter { set; get; }       
        public string SubComponentCounterShortDesc { set; get; }
        public string SubComponentCounterLongDesc { set; get; }
        public decimal? SubComponentCounterScore { set; get; }
        public decimal Score { set; get; }
        public string AdditionalScoreName { set; get; }
        public bool ShowScoreAsNA { set; get; }
        public string CategoryAdditionalScore { set; get; }

        public List<string> ListIdLesson { set; get; }
        public int OrderComponentNo { set; get; }
        public int OrderSubComponentNo { set; get; }
    }

    public class SubjectMappingSubjectLevelTotalScore
    {
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public decimal? TotalScore { set; get; }
        public string Grade { set; get; }
        public string IdLesson { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public decimal SubComponentWeight { set; get; }
        public decimal SubComponentWeightedAverage { set; get; }
        public string IdSubComponentCounter { set; get; }
        public decimal SubComponentCounterScore { set; get; }
    }
}
