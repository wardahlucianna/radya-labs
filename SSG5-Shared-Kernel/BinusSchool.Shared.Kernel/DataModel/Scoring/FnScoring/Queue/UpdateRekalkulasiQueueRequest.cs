using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Scoring.FnScoring.UpdateScore;

namespace BinusSchool.Data.Model.Scoring.FnScoring.Queue
{
    public class UpdateRekalkulasiQueueRequest
    {
        public bool isFromEntryScore { set; get; }
        public List<UpdateRekalkulasiStudentScore_StudentComponentVm> StudentUpdates { set; get; }
        public List<string> SubjectMappingJobsList { set; get; }
    }
            
}
