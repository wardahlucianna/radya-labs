using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingType
{
    public class GetBlockingTypeMenuResult : CodeWithIdVm
    {
        public List<CodeWithIdVm> SubMenu { get; set; }
    }

}
