using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class SaveGeneralMedicalInformationRequest
    {
        public string Id { get; set; }
        public string Mode { get; set; }
        public string HealthCondition { get; set; }
        public string ApprovedMedicine { get; set; }
        public string Allergies { get; set; }
        public string HealthNote { get; set; }
        public string RegularMedication { get; set; }
    }
}
