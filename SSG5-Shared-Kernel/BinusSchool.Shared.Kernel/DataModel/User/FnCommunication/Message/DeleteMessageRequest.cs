using System;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class DeleteMessageRequest
    {
        public string IdUser { get; set; }

        public bool IsDeletePermanent { get; set; }
        public MessageFolder MessageFolder { get; set; }
        public List<string> IdMessage { get; set; }
    }
}
