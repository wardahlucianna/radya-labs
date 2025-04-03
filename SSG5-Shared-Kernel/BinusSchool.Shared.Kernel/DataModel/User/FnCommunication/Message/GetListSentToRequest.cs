using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;


namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetListSentToRequest : CollectionRequest
    {
        public string IdMessage { get; set; }
        public string IdSchool { get; set; }
        public string Search { get; set; }

    }
}
