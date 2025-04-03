namespace BinusSchool.Data.Model
{
    public class AuthUser
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
    }

    public class AuthUserPass : AuthUser
    {
        public string Password { get; set; }
    }

    public class AuthUserToken : AuthUser
    {
        public string Token { get; set; }
    }
}
