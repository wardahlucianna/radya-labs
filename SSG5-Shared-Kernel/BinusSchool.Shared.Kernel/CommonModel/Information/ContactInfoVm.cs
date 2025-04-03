using System.Collections.Generic;

namespace BinusSchool.Common.Model.Information
{
    public class ContactInfoVm
    {
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string MobilePhoneNumber2 { get; set; }
        public string MobilePhoneNumber3 { get; set; }
        public string EmergencyContactRole { get; set; }
        public List<EmergencyContactInfoVm> ListEmergencyContactRole { get; set; }
        public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
    }
}
