using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class GetMeritDemeritDisciplineAprovalResult : CodeWithIdVm
    {
        public string AcademicYear { get; set; }
        public string IdAcademicYear { get; set; }
        public string Level { get; set; }
        public string IdLevel { get; set; }
        public CodeWithIdVm Approval1 { get; set; }
        public CodeWithIdVm Approval2 { get; set; }
        public CodeWithIdVm Approval3 { get; set; }
    }
}
