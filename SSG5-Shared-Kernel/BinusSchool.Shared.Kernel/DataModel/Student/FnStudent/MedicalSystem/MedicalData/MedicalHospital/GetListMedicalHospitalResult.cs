using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalHospital
{
    public class GetListMedicalHospitalResult
    {
        public string IdHospital { get; set; }
        public string HospitalName { get; set; }
        public string HospitalAddress { get; set; }
        public string HospitalPhoneNumber { get; set; }
        public string HospitalEmail { get; set; }
        public bool CanDelete { get; set; }
    }
}
