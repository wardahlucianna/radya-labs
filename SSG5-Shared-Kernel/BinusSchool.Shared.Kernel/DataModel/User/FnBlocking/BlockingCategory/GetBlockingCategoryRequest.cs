using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingCategory
{
    public class GetBlockingCategoryRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
