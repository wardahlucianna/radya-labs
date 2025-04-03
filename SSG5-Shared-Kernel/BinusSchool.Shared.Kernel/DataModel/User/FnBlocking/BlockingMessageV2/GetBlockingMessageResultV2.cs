using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingMessageV2
{
    public class GetBlockingMessageResultV2 : ItemValueVm
    {
        public ItemValueVm School { get; set; }
        public ItemValueVm Category { get; set; }
        public string Content { get; set; }
    }
}
