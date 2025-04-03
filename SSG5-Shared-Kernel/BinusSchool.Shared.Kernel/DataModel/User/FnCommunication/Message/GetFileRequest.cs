using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetFileRequest
    {
        public string IdMessageAttachment { get; set; }

        public string FileName { get; set; }
        public string IdMessage { get; set; }
        public string IdSchool { get; set; }
    }
}