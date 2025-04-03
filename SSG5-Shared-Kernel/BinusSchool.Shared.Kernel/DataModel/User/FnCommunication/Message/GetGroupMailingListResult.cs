using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetGroupMailingListResult : ItemValueVm
    {
        public string GroupName { get; set; }
        public DateTime JoinDate { get; set; }
        public string GroupRole { get; set; }
        public bool CreateMessage { get; set; }
        public List<GroupMember> Members { get; set; }
    }
}
