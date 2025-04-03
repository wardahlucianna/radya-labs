using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Common.Model.Information
{
    public class PersonalParentInfoVm
    {
        public ItemValueVm ParentRole { get; set; }
        public ItemValueVm AliveStatus { get; set; }
        public ItemValueVm LastEducation { get; set; }
        public ItemValueVm BinusianStatus { get; set; }
        public ItemValueVm Relationship { get; set; }
        public string BinusianID { get; set; }
        public string NameForCertificate { get; set; }
    }
}
