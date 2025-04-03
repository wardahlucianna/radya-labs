using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup
{
    public class CreateMasterGroupRequest
    {
        public string IdSchool { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public bool? Status { get; set; }
    }
}
