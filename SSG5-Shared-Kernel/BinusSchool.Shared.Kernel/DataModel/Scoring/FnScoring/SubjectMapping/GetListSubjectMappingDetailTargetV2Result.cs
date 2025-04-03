using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping
{
    public class GetListSubjectMappingDetailTargetV2Result
    {
        public List<GetListSubjectMappingDetailTargetV2Result_SubjectSemester> SubjectSemesterList { get; set; }

    }
    public class GetListSubjectMappingDetailTargetV2Result_SubjectSemester
    {
        public ItemValueVm SubjectSemester { get; set; }
        public List<GetListSubjectMappingDetailTargetV2Result_Term> TermList { get; set; }
    }
    public class GetListSubjectMappingDetailTargetV2Result_Term
    {
        public ItemValueVm Term { get; set; }
        public List<GetListSubjectMappingDetailTargetV2Result_Component> ComponentList { get; set; }
    }

    public class GetListSubjectMappingDetailTargetV2Result_Component
    {
        public ItemValueVm Component { get; set; }
        public List<GetListSubjectMappingDetailTargetV2Result_SubComponent> SubComponentList { get; set; }
    }

    public class GetListSubjectMappingDetailTargetV2Result_SubComponent
    {
        public ItemValueVm SubComponent { get; set; }
        public List<GetListSubjectMappingDetailTargetV2Result_SubComponentCounter> SubcomponentCounterList { get; set; }
    }

    public class GetListSubjectMappingDetailTargetV2Result_SubComponentCounter
    {
        public ItemValueVm SubComponentCounter { get; set; }
    }

}
