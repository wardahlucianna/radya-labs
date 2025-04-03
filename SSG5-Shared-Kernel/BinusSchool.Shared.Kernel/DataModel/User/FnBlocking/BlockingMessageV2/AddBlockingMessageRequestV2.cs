using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingMessageV2
{
    public class AddBlockingMessageRequestV2
    {
        public string IdSchool { get; set; }
        public string IdCategory { get; set; }
        public string Content { get; set; }
    }
}
