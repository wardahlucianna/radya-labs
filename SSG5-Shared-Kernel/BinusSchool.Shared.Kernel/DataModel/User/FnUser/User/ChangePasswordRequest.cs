
namespace BinusSchool.Data.Model.User.FnUser.User
{
    public class ChangePasswordRequest
    {
        public string RequestCode { get; set; }
        public string NewPassword { get; set; }
    }
}
