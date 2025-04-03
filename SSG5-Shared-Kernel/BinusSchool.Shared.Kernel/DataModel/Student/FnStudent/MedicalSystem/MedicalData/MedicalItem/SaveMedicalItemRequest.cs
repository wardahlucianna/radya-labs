using System;
using System.Collections.Generic;
using System.Text;
using NPOI.OpenXmlFormats.Dml;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData.MedicalItem
{
    public class SaveMedicalItemRequest
    {
        public string IdSchool { get; set; }
        public string IdMedicalItem { get; set; }
        public string MedicalItemName { get; set; }
        public string IdMedicalItemType { get; set; }
        public bool IsCommonDrug { get; set; }
        public string IdMedicineType { get; set; }
        public string IdMedicineCategory { get; set; }
        public string IdDosageType { get; set; }
    }
}
