using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryV2.ComponentData
{
    public class GetComponentPYPPOIResult
    {
        public int Semester { get; set; }
        public List<GetComponentPYPPOIResult_UnitOfInquiry> ListOfInquiry { get; set; }
    }

    public class GetComponentPYPPOIResult_UnitOfInquiry
    {
        public int Semester { get; set; }
        public ItemValueVm UnitInq { get; set; }
        public ItemValueVm CentralIdea { get; set; }
        public List<GetComponentPYPPOIResult_InfoOfInquiry> ListOfInfo { get; set; }
        public string Comment { get; set; }
    }
    public class GetComponentPYPPOIResult_InfoOfInquiry
    {
        public ItemValueVm InfoInq { get; set; }
    }
}
