using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class GetSubjectMappingDetailResult
    {
        public string IdSubjectMappingDetail { get; set; }
        public GetSubjectMappingDetailResult_Target SubjectMappingTarget { get; set; }
        public GetSubjectMappingDetailResult_Source SubjectMappingSource { get; set; }
    }

    public class GetSubjectMappingDetailResult_Target
    {
        public GetSubjectMappingDetailResult_SubjectVm Subject { get; set; }
        public ItemValueVm Component { get; set; }
        public ItemValueVm SubComponent { get; set; }
    }

    public class GetSubjectMappingDetailResult_Source
    {
        public GetSubjectMappingDetailResult_SubjectVm Subject { get; set; }
        public ItemValueVm Component { get; set; }
        public ItemValueVm SubComponent { get; set; }
        public decimal Weight { get; set; }
    }

    public class GetSubjectMappingDetailResult_SubjectVm
    {
        public string IdSubjectScoreSetting { get; set; }
        public string Description { get; set; }
    }
}
