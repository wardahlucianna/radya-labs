using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingCategory
{
    public class UpdateBlockingCategoryRequest
    {
        public string IdBlockingCategory { get; set; }
        public string BlockingCategory { get; set; }
        public List<BlockingType> IdsBlockingType { get; set; }
        public List<AssignUser> IdsAssignUser { get; set; }
    }
}
