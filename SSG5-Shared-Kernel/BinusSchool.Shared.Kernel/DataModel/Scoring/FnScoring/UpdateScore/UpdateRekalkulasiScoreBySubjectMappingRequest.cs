using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.UpdateScore
{
    public class UpdateRekalkulasiScoreBySubjectMappingRequest
    {
        public string idStudent { set; get; }
        public string IdSubjectScoreSetting { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
    }

    public class UpdateRekalkulasiScoreBySubjectMappingVm
    {
        public string idStudent { set; get; }
        public string IdSubject { set; get; }
        public string IdComponent { set; get; }
        public string IdSubComponent { set; get; }
        public int Level { set; get; }
    }
}
