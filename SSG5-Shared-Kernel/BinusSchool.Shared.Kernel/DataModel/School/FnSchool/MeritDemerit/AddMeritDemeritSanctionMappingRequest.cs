using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class AddMeritDemeritSanctionMappingRequest
    {
        public string IdAcademicYear { get; set; }
        public string NameSanction { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public List<AttentionBy> Attention { get; set; }

    }

    public class AttentionBy
    {
        public string IdRole { get; set; }
        public string IdPosition { get; set; }
        public string IdUser { get; set; }
    }
}
