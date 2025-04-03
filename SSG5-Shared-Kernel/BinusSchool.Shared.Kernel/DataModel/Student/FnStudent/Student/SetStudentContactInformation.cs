using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class SetStudentContactInformation
    {
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string MobilePhoneNumber2 { get; set; }
        //public string MobilePhoneNumber3 { get; set; }
        //public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public ItemValueVm EmergencyContactRole { get; set; }
    }
}
