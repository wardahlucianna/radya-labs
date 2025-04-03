using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class AddMedicalRecordEntryOtherPatientRequest
    {
        public string IdSchool { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
