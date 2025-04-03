using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingCategory
{
    public class GetBlockingCategoryDetailResult : ItemValueVm
    {
        public string BlockingCategory { get; set; }
        public List<ItemValueVm> BlockingType { get; set; }
        public List<AssignUserList> AssignUser { get; set; }
    }

    public class AssignUserList : CodeWithIdVm
    {
        public string DisplayName { get; set; }
        public string BinusianID { get; set; }
        public string Username { get; set; }
    }
}
