using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class GetListSubjectMappingDetailSourceResult
    {
        public List<GetListSubjectMappingDetailSourceResult_Subject> SubjectList { get; set; }
    }
    public class GetListSubjectMappingDetailSourceResult_Subject
    {
        public GetListSubjectMappingDetailSourceResult_SubjectVm Subject { get; set; }
        public List<GetListSubjectMappingDetailSourceResult_Component> ComponentList { get; set; }
    }

    public class GetListSubjectMappingDetailSourceResult_Component
    {
        public ItemValueVm Component { get; set; }
        public List<GetListSubjectMappingDetailSourceResult_SubComponent> SubComponentList { get; set; }
    }

    public class GetListSubjectMappingDetailSourceResult_SubComponent
    {
        public ItemValueVm SubComponent { get; set; }
    }
    public class GetListSubjectMappingDetailSourceResult_SubjectVm
    {
        public string IdSubjectScoreSetting { get; set; }
        public string Description { get; set; }
    }
}
