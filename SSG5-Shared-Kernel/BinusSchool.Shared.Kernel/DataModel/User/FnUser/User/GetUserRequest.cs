namespace BinusSchool.Data.Model.User.FnUser.User
{
    public class GetUserRequest : CollectionSchoolRequest
    {
        public string IdRole { get; set; }
        public string RoleGroupCode { get; set; }
    }
}
