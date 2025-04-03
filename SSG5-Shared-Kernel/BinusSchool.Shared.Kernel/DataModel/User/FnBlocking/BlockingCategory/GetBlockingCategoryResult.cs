using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingCategory
{
    public class GetBlockingCategoryResult : ItemValueVm
    {
        public string BlockingCategory { get; set; }

        public string BlockingType { get; set; }
    }

    public class GetBlockingCategoryQueryResult : ItemValueVm
    {
        public string BlockingCategory { get; set; }

        public List<GetBlockingCategoryType> BlockingType { get; set; }
    }

    public class GetBlockingCategoryType : ItemValueVm
    {
        public string Category { get; set; }
        public int Order { get; set; }
    }
}
