using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class UnsendMessageRequest
    {
        public string IdUser { get; set; }
        public string IdMessage { get; set; }
        public string IdSchool { get; set; }
    }
}
