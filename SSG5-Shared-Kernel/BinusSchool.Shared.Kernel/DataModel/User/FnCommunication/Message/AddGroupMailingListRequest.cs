using System.Collections.Generic;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class AddGroupMailingListRequest
    {
        public string GroupName { get; set; }
        public string Description { get; set; }
        public List<MemberList> MemberLists { get; set; }
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
    }

    public class MemberList
    {
        public string Id { get; set; }
        public string IdUser { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public bool CreateMessage { get; set; }

    }
}
