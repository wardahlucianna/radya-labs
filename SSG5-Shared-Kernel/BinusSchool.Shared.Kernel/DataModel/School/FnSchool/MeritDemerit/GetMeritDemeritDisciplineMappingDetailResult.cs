using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class GetMeritDemeritDisciplineMappingDetailResult :  CodeWithIdVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public CodeWithIdVm Level { get; set; }
        public CodeWithIdVm Grade { get; set; }
        public MeritDemeritCategory Catagory { get; set; }
        public NameValueVm LavelInfraction { get; set; }
        public string NameDiscipline { get; set; }
        public string Point { get; set; }
    }
}
