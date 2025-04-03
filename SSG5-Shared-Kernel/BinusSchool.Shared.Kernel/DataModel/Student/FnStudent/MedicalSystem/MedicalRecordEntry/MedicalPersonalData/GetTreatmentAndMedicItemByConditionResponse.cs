using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class GetTreatmentAndMedicItemByConditionResponse
    {
        public List<GetTreatmentAndMedicItemByConditionResponse_Treatment> Treatments { get; set; }
        public List<GetTreatmentAndMedicItemByConditionResponse_MedicItem> MedicItems { get; set; }
    }

    public class GetTreatmentAndMedicItemByConditionResponse_Treatment : ItemValueVm { }

    public class GetTreatmentAndMedicItemByConditionResponse_MedicItem : ItemValueVm
    {
        public string MedicalItemType { get; set; }
        public string DosageType { get; set; }
    }
}
