using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnBlocking.BlockingMessage
{
    public class UpdateBlockingMessageRequest
    {
        public string Id { get; set; }
        public string IdSchool { get; set; }
        public string BlockingMessage { get; set; }
    }
}
