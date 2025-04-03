using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.DetailsConditionTreatmentMedicationEntry
{
    public class GetDetailsConditionDataRequest
    {
        public DateTime? CheckInDate { get; set; }
        public string Id { get; set; }
        public string IdSchool { get; set; }
    }
}
