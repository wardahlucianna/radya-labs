using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SubjectSplitMapping
{
    public class GetSubjectSplitMappingDetailResult
    {
        public string IdSubjectType { get; set; }
        public string SubjectTypeName { get; set; }
        public List<GetSubjectSplitMappingDetail_SubjectVm> SubjectChilds { get; set; }

    }

    public class GetSubjectSplitMappingDetail_SubjectVm
    {
        public string IdSubject { get; set; }
        public string SubjectName { get; set; }
        public bool Checked { get; set; }
    }
}
