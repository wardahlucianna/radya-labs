using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad
{
    public class GetNonTeachLoadDetailResult : DetailResult2
    {
        public GetNonTeachLoadDetailResult()
        {
            Hierarchies = new List<GetRecentHierarchy>();
        }
        public CodeWithIdVm Acadyear { get; set; }
        public CodeWithIdVm Position { get; set; }
        public AcademicType Category { get; set; }
        public int Load { get; set; }
        [IgnoreDataMember]
        public string Parameter { get; set; }
        public List<GetRecentHierarchy> Hierarchies { get; set; }
    }
}
