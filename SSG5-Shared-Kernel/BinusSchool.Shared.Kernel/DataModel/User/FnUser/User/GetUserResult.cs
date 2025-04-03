using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.User
{
    public class GetUserResult : CodeWithIdVm
    {
        public string DisplayName { get; set; }
        public AuditableResult Audit { get; set; }
        public string ShortName { get; set; }
    }
}
