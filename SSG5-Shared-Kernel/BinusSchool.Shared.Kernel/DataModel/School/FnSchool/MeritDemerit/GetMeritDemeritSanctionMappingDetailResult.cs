using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class GetMeritDemeritSanctionMappingDetailResult : CodeWithIdVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public string NameSanction { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public List<DetailAttentionBy> AttentionBy { get; set; }
    }

    public class DetailAttentionBy
    {
        public CodeWithIdVm IdRole { get; set; }
        public CodeWithIdVm IdUser { get; set; }
        public DetailAttentionByIdTeacherPostion IdTeacherPostion { get; set; }
    }

    public class DetailAttentionByIdTeacherPostion : CodeWithIdVm
    {
        public string IdPostion { get; set; }
    }
}
