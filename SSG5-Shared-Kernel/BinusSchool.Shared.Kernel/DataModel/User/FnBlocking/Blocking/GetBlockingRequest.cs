using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.Blocking
{
    public class GetBlockingRequest : CollectionRequest
    {
        public string IdStudent { get; set; }
        public string IdSchool { get; set; }
        public string IdFeature { get; set; } 
    }
}
