using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingCategory
{
    public class GetBlockingByUserRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdUser { get; set; }
    }
}
