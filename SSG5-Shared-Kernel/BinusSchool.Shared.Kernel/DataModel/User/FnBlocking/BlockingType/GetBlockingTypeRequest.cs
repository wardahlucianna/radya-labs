using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingType
{
    public class GetBlockingTypeRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
