using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup
{
    public class UpdateMasterGroupRequest
    {
        public NameValueVm Group { get; set; }
        public string IdSchool { get; set; }
        public string Description { get; set; }
        public bool? Status { get; set; }
    }
}
