using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.DetailsConditionTreatmentMedicationEntry
{
    public class GetDetailsCondtionDataResult
    {
        public DateTime CheckInDate { get; set; }
        public GetDetailsCondtionDataResult_Data Data { get; set; }
    }

    public class GetDetailsCondtionDataResult_Data
    {
        public string IdDetailsCondition { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public List<ItemValueVm> Conditions { get; set; }
        public List<ItemValueVm> Treatments { get; set; }
        public List<GetDetailsCondtionDataResult_MedicalItems> MedicalItems { get; set; }
        public string? TeacherInCharge { get; set; }
        public string? Location { get; set; }
        public ItemValueVm? Hospital { get; set; }
        public bool DismissedHome { get; set; }
        public string? DetailsNotes { get; set; }
    }

    public class GetDetailsCondtionDataResult_MedicalItems
    {
        public string IdMedicalItem { get; set; }
        public string Description { get; set; }
        public string MedicalItemType { get; set; }
        public int DosageAmount { get; set; }
        public string DosageType { get; set; }
    }
}
