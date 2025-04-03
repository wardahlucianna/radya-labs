using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingType
{
    public class GetBlockingTypeDetailResult : ItemValueVm
    {
        public string BlockingType { get; set; }

        public MenuBlockingTypeDetail Menu { get; set; }

    }

    public class MenuBlockingTypeDetail : CodeWithIdVm
    {
        public List<SubMenuBlockingTypeDetail> SubMenu { get; set; }
    }

    public class SubMenuBlockingTypeDetail : CodeWithIdVm
    {
        public bool IsChecked { get; set; }
    }
}
