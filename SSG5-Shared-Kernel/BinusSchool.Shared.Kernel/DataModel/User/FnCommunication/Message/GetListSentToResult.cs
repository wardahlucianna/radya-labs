using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;


namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetListSentToResult : ItemValueVm
    {
        public List<ListUserSentTo> ListUserSentTos { get; set; }
        public List<ListMemberSentTo> ListMemberSentTos { get; set; }

    }

    public class ListUserSentTo : ItemValueVm
    {
        public string Role { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string BinusianID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
    }

    public class ListMemberSentTo : ItemValueVm
    {
        public string GroupName { get; set; }
        public List<ListUserSentTo> ListGroupMembers { get; set; }
    }

}
