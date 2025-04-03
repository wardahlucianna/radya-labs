using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;


namespace BinusSchool.Data.Model.User.FnCommunication.Message
{
    public class GetUserBySpecificFilterResult : ItemValueVm
    {
        public string DisplayName { get; set; }
        public AuditableResult Audit { get; set; }
        public string Role { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string BinusianID { get; set; }
        public string Username { get; set; }
    }

    public class GetUserBySpecificFilterQueriable : ItemValueVm
    {
        public string DisplayName { get; set; }
        public AuditableResult Audit { get; set; }
        public string Role { get; set; }
        public string IdLevel { get; set; }
        public string Level { get; set; }
        public string IdGrade { get; set; }
        public string IdAcademicYear { get; set; }
        public string Grade { get; set; }
        public int Semester { get; set; }
        public string IdHomeroom { get; set; }
        public string Homeroom { get; set; }
        public string BinusianID { get; set; }
        public string Username { get; set; }
    }
}
