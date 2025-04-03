using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital
{
    public class SaveMedicalHospitalRequest
    {
        public string IdSchool { get; set; }
        public string IdHospital { get; set; }
        public string HospitalName { get; set; }
        public string HospitalAddress { get; set; }
        public string HospitalPhone { get; set; }
        public string HospitalEmail { get; set; }
    }
}
