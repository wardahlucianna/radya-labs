using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser
{
    public class GetListUserForLoginAsResult
    {
        public string BinusianID { get; set; }
        public string BinusianUsername { get; set; }
        public string BinusianDisplayName { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public bool IsStudentOrParent { get; set; }
        public bool CanLoginAsStudent { get; set; }
        public bool CanLoginAsParent { get; set; }
        public string ParentUsername { get; set; }
        public string ParentDisplayName { get; set; }
    }

    public class GetListUserForLoginAsResult_RoleUser
    {
        public string IdRoleGroup { get; set; }
        public string RoleGroupCode { get; set; }
        public string IdRole { get; set; }
        public string RoleCode { get; set; }
        public string IdUser { get; set; }
        public string IdStudent { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
    }

}
