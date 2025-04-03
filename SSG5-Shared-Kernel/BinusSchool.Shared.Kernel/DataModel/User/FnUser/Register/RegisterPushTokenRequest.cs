using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.User.FnUser.Register
{
    public class RegisterPushTokenRequest
    {
        public AppPlatform Platform { get; set; }
        public string FirebaseToken { get; set; }
    }
}
