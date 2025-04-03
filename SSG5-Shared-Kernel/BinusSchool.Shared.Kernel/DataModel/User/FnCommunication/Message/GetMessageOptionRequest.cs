using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetMessageOptionRequest
    {
        public string IdSchool { get; set; }
        public string Code { get; set; }
    }
}