using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup
{
    public class GetMasterGroupResult : CodeWithIdVm
    {
        public NameValueVm Group { get; set; }
        public ItemValueVm School { get; set; }
        public bool Status { get; set; }
    }
}
