using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser
{
    public class GetListUserForLoginAsRequest : CollectionRequest
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdRoleGroup { get; set; }
        public string RoleGroupCode { get; set; }
        public string? IdLevel { get; set; }
        public string? IdGrade { get; set; }
        public string? IdHomeroom { get; set; }
    }
}
