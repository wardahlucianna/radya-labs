using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class UpdateStudentContactInformationRequest
    {
        public string IdStudent { get; set; }
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string MobilePhoneNumber2 { get; set; }
        //public string MobilePhoneNumber3 { get; set; }
        public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string EmergencyContactRole { get; set; }
        public string EmergencyContactRoleDesc { get; set; }
        public int IsParentUpdate { get; set; }
    }
}
