using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.EmergencyStudentContact
{
    public class GetEmergencyStudentContactResult
    {
        public string ParentName { get; set; }
        public string ParentRole { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool PrimaryEmergencyContact { get; set; }
    }
}
