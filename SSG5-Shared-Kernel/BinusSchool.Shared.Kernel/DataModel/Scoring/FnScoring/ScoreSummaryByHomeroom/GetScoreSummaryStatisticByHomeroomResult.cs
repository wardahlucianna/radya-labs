using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByHomeroom
{
    public class GetScoreSummaryStatisticByHomeroomResult
    {
        public ItemValueVm Class { set; get; }
        public int StudentCount { set; get; }
        public int StudentSubmitted { set; get; }
        public int StudentUnsubmitted { set; get; }
        public decimal ClassAvg { set; get; }
        public decimal Highest { set; get; }
        public decimal Lowest { set; get; }
        public decimal StdDeviation { set; get; }
        public PercentageData PercentagePass { set; get; }
        public PercentageData PercentageFail { set; get; }
        public List<ScoreStatisticData> StatisticList { set; get; }
    }

    public class PercentageData
    {
        public int StudentCount { set; get; }
        public double Percentage { set; get; }
        public List<StudentScoreData> StudentList { set; get; }
    }

    public class ScoreStatisticData
    {
        public int StudentCount { set; get; }
        public string IdScoreStatistic { set; get; }
        public List<StudentScoreData> StudentList { set; get; }
    }

    public class StudentScoreData
    {
        public string StudentName { set; get; }
        public string StudentId { set; get; }
        public decimal Score { set; get; }
        public string LastYearStudentStatus { set; get; }
    }
}

