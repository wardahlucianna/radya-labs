using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalCondition
{
    public class SaveMedicalConditionRequest
    {
        public string? IdMedicalCondition { get; set; }
        public string IdSchool { get; set; }
        public string MedicalConditionName { get; set; }
        public List<string> IdMedicalItem { get; set; }
        public List<string> IdMedicalTreatment { get; set; }

    }
}
