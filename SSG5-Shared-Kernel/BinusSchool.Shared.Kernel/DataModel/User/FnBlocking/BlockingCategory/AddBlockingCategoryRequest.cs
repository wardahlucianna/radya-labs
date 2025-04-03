using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingCategory
{
    public class AddBlockingCategoryRequest
    {
        public string IdSchool { get; set; }
        public string BlockingCategory { get; set; }
        public List<BlockingType> IdsBlockingType { get; set; }
        public List<AssignUser> IdsAssignUser { get; set; }
    }

    public class BlockingType
    {
        public string Id { get; set; }
    }
    public class AssignUser
    {
        public string Id { get; set; }
    }
}
