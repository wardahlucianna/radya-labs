using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalDoctor
{
    public abstract class GetMedicalDoctorResult
    {
        public string IdMedicalDoctor { get; set; }
        public string DoctorName { get; set; }
        public string DoctorAddress { get; set; }
        public string DoctorPhoneNumber { get; set; }
        public string DoctorEmail { get; set; }
    }

    public class GetListMedicalDoctorResult : GetMedicalDoctorResult
    {
        public bool CanDelete { get; set; }
    }

    public class GetDetailMedicalDoctorResult : GetMedicalDoctorResult
    {
        public GetDetailMedicalDoctorResult_Hospital Hospital { get; set; }
    }

    public class GetDetailMedicalDoctorResult_Hospital
    {
        public string IdMedicalHospital { get; set; }
        public string HospitalName { get; set; }
        public string HospitalAddress { get; set; }
        public string HospitalPhoneNumber { get; set; }
        public string HospitalEmail { get; set; }
    }
}
