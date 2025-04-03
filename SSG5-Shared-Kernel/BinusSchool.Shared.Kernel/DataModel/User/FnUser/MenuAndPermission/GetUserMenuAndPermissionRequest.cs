
namespace BinusSchool.Data.Model.User.FnUser.MenuAndPermission
{
    public class GetUserMenuAndPermissionRequest
    {
        public string IdSchool { get; set; }
        public string IdUser { get; set; }
        public string IdRole { get; set; }
        public bool IsMobile { get; set; }
    }
}
