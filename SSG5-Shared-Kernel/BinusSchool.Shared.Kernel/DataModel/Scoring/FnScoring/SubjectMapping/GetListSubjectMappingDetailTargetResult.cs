using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class GetListSubjectMappingDetailTargetResult
    {
        public List<GetListSubjectMappingDetailTargetResult_Subject> SubjectList { get; set; }
    }
    public class GetListSubjectMappingDetailTargetResult_Subject
    {
        public GetListSubjectMappingDetailTargetResult_SubjectVm Subject { get; set; }
        public List<GetListSubjectMappingDetailTargetResult_Component> ComponentList { get; set; }
    }

    public class GetListSubjectMappingDetailTargetResult_Component
    {
        public ItemValueVm Component { get; set; }
        public List<GetListSubjectMappingDetailTargetResult_SubComponent> SubComponentList { get; set; }
    }

    public class GetListSubjectMappingDetailTargetResult_SubComponent
    {
        public ItemValueVm SubComponent { get; set; }
        public List<GetListSubjectMappingDetailTargetResult_SubComponentCounter> SubcomponentCounterList { get; set; }
    }

    public class GetListSubjectMappingDetailTargetResult_SubComponentCounter 
    {
        public ItemValueVm SubComponentCounter { get; set; }
    }

    public class GetListSubjectMappingDetailTargetResult_SubjectVm
    {
        public string IdSubjectScoreSetting { get; set; }
        public string Description { get; set; }
    }
}
