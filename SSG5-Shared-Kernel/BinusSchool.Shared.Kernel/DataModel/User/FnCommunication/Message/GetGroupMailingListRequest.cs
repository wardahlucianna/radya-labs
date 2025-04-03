using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetGroupMailingListRequest : CollectionRequest
    {
        public string GroupName { get; set; }
        public string IdUser { get; set; }
        public bool? IsCreateMessage { get; set; }
    }
}
