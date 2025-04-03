using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingMessageV2
{
    public class GetBlockingMessageRequestV2 : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdCategory { get; set; }
    }
}
