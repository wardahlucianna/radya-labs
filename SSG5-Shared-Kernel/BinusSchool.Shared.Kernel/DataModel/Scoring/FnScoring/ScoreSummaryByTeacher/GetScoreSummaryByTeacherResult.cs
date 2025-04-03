using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByTeacher
{
    public class GetScoreSummaryByTeacherResult
    {
        public int TotalCounter { get; set; }
        public int TotalSubmitted { get; set; }
        public int TotalPending { get; set; }
        public int TotalUnsubmitted { get; set; }
        public ItemValueVm Department { get; set; }
        public List<GetScoreSummaryByTeacherResult_TeacherCounter> TeacherList { get; set; }
    }

    public class GetScoreSummaryByTeacherResult_TeacherCounter
    {
        public ItemValueVm Teacher { get; set; }
        public ItemValueVm Subject { get; set; }
        public ItemValueVm SubjectType { get; set; }
        public int TotalCounter { get; set; }
        public int TotalSubmitted { get; set; }
        public int TotalPending { get; set; }
        public int TotalUnsubmitted { get; set; }
    }
}
