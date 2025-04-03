using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class GetMeritDemeritSanctionMappingResult : CodeWithIdVm
    {
        public string AcademicYear { get; set;}
        public string NameSanction { get; set;}
        public int MinSanction { get; set;}
        public int MaxSanction { get; set;}
        public string AttentionBy { get; set;}
    }
}
