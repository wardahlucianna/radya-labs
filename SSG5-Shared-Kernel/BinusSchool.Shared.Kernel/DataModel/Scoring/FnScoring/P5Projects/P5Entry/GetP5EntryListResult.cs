using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.P5Projects.P5Entry
{
    public class GetP5EntryListResult : ItemValueVm
    {
        public string IdP5Project { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Theme { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public string Score { get; set; }
        public int ScoreSubmitted { get; set; }
        public GetP5EntryListResult_LastMofiedVm LastModified { get; set; }
    }

    public class GetP5EntryListResult_LastMofiedVm
    {
        public DateTime Date { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }

}
