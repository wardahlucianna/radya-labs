using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByLesson
{
    public class GetStudentScoreSubjectByLessonResult
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
        public List<ScoreViewSubject_SubComponentCounterVm> CounterScoreList { set; get; }
        public decimal? weightedAverage { set; get; }

    }

    public class ScoreViewSubject_SubComponentCounterVm
    {
        public string IdSubComponentCounter { set; get; }
        public string SubComponentCounterScore { set; get; }


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

}
