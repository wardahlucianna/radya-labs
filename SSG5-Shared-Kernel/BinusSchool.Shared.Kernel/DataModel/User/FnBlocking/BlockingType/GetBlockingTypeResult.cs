using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingType
{
    public class GetBlockingTypeResult : ItemValueVm
    {
        public string BlockingType { get; set; }

        public string Menu { get; set; }

        public string Category { get; set; }

        public bool CanDelete { get; set; }
        public bool CanEdit { get; set; }
        public string SubMenu { get; set; }

    }

}
