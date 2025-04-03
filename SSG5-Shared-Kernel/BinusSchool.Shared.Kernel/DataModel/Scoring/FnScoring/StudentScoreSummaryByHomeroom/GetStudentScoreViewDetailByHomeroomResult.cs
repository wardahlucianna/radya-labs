using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scoring.FnScoring.EntryScore;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByHomeroom
{
    public class GetStudentScoreViewDetailByHomeroomResult
    {
        public string SubjectName { get; set; }
        public string TotalScore { set; get; }
        public string ScoreColor { set; get; }
        public List<StudentScoreCounterVm> StudentCounterScoreList { set; get; }
        public bool? OverallScore { set; get; }
    }

    public class StudentScoreCounterVm
    {
        public string Term { set; get; }
        public string ComponentName { set; get; }
        public string SubComponentName { set; get; }
        public string Counter { set; get; }
        public string IdScore { set; get; }
        public decimal? Score { set; get; }
        public string ViewScore { set; get; }
        public string TextScore { set; get; }
        public int OrderNumberComponent { set; get; }
        public int OrderNumberSubComponent { set; get; }
    }

    public class SubjectScoreSettingVm
    {
        public string IdSubject { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdGrade { set; get; }
        public int Semester { set; get; }        
        public List<SubjectScoreLegendVm> SubjectScoreLegendList { set; get; }
    }



}
