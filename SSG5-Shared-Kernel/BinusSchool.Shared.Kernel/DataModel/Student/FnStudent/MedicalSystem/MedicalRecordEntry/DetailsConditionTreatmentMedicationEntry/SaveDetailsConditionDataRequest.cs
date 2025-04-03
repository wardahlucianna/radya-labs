using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.DetailsConditionTreatmentMedicationEntry
{
    public class SaveDetailsConditionDataRequest
    {
        public DateTime CheckInDate { get; set; }
        public SaveDetailsConditionDataRequest_Data Data { get; set; }
        public string Id { get; set; }
        public string IdSchool { get; set; }
    }   

    public class SaveDetailsConditionDataRequest_Data
    {
        public string? IdDetailsCondition { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public List<string> IdConditions { get; set; }
        public List<string> IdTreatments { get; set; }
        public List<SaveDetailsConditionDataRequest_Medication> Medication { get; set; }
        public string? TeacherInCharge { get; set; }
        public string? Location { get; set; }
        public string? IdHospital { get; set; }
        public bool DismissedHome { get; set; }
        public string? DetailsNotes { get; set; }
    }

    public class SaveDetailsConditionDataRequest_Medication
    {
        public string IdMedicalItem { get; set; }
        public int DosageAmount { get; set; }
    }
}
