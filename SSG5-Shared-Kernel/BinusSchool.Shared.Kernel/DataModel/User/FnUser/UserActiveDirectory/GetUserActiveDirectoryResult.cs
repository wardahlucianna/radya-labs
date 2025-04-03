using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.UserActiveDirectory
{
    public class GetUserActiveDirectoryResult : NameValueVm
    {
        public string UserPrincipalName { get; set; }
        public string Email { get; set; }
        public string CompanyName { get; set; }
    }
}
