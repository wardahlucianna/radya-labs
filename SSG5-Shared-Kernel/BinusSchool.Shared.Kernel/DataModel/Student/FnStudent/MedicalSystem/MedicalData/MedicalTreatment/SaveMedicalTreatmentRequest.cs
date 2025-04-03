using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalTreatment
{
    public class SaveMedicalTreatmentRequest
    {
        public string? IdMedicalTreatment { get; set; }
        public string MedicalTreatmentName { get; set; }
        public string IdSchool { get; set; }
    }
}
