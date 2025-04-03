using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor
{
    public class SaveMedicalDoctorRequest
    {
        public string? IdMedicalDoctor { get; set; }
        public string DoctorName { get; set; }
        public string DoctorAddress { get; set; }
        public string DoctorPhoneNumber { get; set; }
        public string DoctorEmail { get; set; }
        public string IdMedicalHospital { get; set; }
        public string IdSchool { get; set; }
    }
}
