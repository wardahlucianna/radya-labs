using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetGroupMailingListDetailsResult : ItemValueVm
    {
        public string GroupName { get; set; }
        public string GroupRole { get; set; }
        public string IdUser { get; set; }
        public string OwnerGroup { get; set; }
        public string UserName { get; set; }
        public string CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public string GroupDescripction { get; set; }
        public List<GroupMember> GroupMembers { get; set; }
        public List<string> IdListMember { get; set; }
    }

    public class GroupMember
    {
        public string Id { get; set; }
        public string IdUser { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string BinusianID { get; set; }
        public bool CreateMessage { get; set; }
        

    }
}
