using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolsEvent
{
    public class GetDefaultEventApproverSettingResult : CodeWithIdVm
    {
        public bool IsSetDefaultApprover1 { get; set;}
        public bool IsSetDefaultApprover2 { get; set;}
        public CodeWithIdVm UserApprover1 { get; set; }
        public CodeWithIdVm UserApprover2 { get; set; }
    }
}