using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class GetListSubjectMappingDetailSourceV2Result
    {
        public List<GetListSubjectMappingDetailSourceV2Result_SubjectSemester> SubjectSemesterList { get; set; }

    }
    public class GetListSubjectMappingDetailSourceV2Result_SubjectSemester
    {
        public ItemValueVm SubjectSemester { get; set; }
        public List<GetListSubjectMappingDetailSourceV2Result_Term> TermList { get; set; }
    }
    public class GetListSubjectMappingDetailSourceV2Result_Term
    {
        public ItemValueVm Term { get; set; }
        public List<GetListSubjectMappingDetailSourceV2Result_Component> ComponentList { get; set; }
    }

    public class GetListSubjectMappingDetailSourceV2Result_Component
    {
        public ItemValueVm Component { get; set; }
        public List<GetListSubjectMappingDetailSourceV2Result_SubComponent> SubComponentList { get; set; }
    }

    public class GetListSubjectMappingDetailSourceV2Result_SubComponent
    {
        public ItemValueVm SubComponent { get; set; }
    }
}
