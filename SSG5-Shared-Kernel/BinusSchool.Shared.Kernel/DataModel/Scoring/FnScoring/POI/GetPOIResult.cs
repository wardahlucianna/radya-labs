using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.POI
{
    public class GetPOIResult
    {
        public List<GetPOIResult_Score> Scores { get; set; }
        public bool IsLocked { get; set; }
        public bool IsInPeriod { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCanEdit { get; set; }
    }

    public class GetPOIResult_Score
    {
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string Comment { get; set; }
        public int MaxCommentLength { get; set; }
    }
}
