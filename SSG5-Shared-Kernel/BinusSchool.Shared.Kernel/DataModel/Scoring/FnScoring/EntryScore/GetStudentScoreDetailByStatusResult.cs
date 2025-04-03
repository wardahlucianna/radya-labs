using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.EntryScore
{
    public class GetStudentScoreDetailByStatusResult
    {
        public List<StudentScoreDetailVm> StudentScoreDetailList { set; get; }
    }
    public class StudentScoreDetailVm
    {
        public string ClassIdGenerated { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public string ComponentName { set; get; }
        public string SubComponentName { set; get; }        
        public string SubComponentCounterName { set; get; }
        public string Teacher { set; get; }
        public string IdScore { set; get; }
    }

    public class StudentScoreDetailWithIDVm
    {
        public string ClassIdGenerated { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public string ComponentName { set; get; }
        public string SubComponentName { set; get; }
        public string IdSubComponentCounter { set; get; }
        public string SubComponentCounterName { set; get; }
        public string Teacher { set; get; }
        public string IdScore { set; get; }
        public DateTime? StartDate { set; get; }
        public DateTime? EndDate { set; get; }
    }
}
