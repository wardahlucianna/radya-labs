using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.MeritDemerit
{
    public class GetMeritDemeritLevelInfractionResult : CodeWithIdVm
    {
        public string NameLavelInfraction { get; set; }
        public string Parent { get; set; }
        public bool IsApproval { get; set; }
        public bool IsDisabledChecked { get; set; }
        public bool IsDisabledDelete { get; set; }

    }
}
