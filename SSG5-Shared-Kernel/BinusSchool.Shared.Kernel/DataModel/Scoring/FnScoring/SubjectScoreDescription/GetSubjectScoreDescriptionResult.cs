using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectScoreDescription
{
    public class GetSubjectScoreDescriptionResult
    {
        public List<GetSubjectScoreDescriptionResult_Pagination> AllPaginationData { get; set; }
        public List<string> AllIdByFilter { get; set; }
    }

    public class GetSubjectScoreDescriptionResult_Pagination : ItemValueVm
    {
        public string Level { get; set; }
        public string Homeroom { get; set; }
        public string StudentData { get; set; }
        public string SubjectType { get; set; }
        public string Subject { get; set; }
        public string SubjectLevel { get; set; }
        public string Title { get; set; }
        public string LastUpdated { get; set; }
        public string Period { get; set; }
        public string IdSubjectScoreDescription { get; set; }
    }
}
