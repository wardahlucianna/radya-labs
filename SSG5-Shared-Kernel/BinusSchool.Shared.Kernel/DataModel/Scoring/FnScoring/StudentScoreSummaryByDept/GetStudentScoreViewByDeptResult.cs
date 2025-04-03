using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.StudentScoreSummaryByDept
{
    public class GetStudentScoreViewByDeptResult
    {
        public ItemValueVm ClassID { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public List<StudentScoreVm> StudentScoreList { set; get; }
        public List<WeightedAverage> WeightedScoreList { get; set; }
        public decimal? Total { set; get; }
        public string Grade { set; get; }
    }

    public class StudentScoreVm
    {
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public string IdSubComponentCounter { set; get; }
        public string IdScore { set; get; }
        public decimal? SubComponentCounterScore { set; get; }
    }

    public class WeightedAverage
    {
        public string IdSubComponent { set; get; }
        public decimal? WeightedAverageScore { get; set; }
    }
}
