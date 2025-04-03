using System.Collections.Generic;

namespace BinusSchool.Data.Model.User.FnBlocking.StudentBlocking
{
    public class UpdateStudentUnBlockingRequest
    {
        public string IdBlockingCategory { get; set; }
        public string IdBlockingType { get; set; }
        public List<UserUnBlocking> IdUsers { get; set; }
    }

    public class UserUnBlocking
    {
        public string IdUser { get; set; }
    }
}
