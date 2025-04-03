using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class GetTreatmentAndMedicItemByConditionRequest
    {
        public string IdSchool { get; set; }
        public List<string> IdCondition { get; set; }
    }
}
