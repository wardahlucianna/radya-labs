using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry
{
    public class GetMedicalRecordEntryClinicVisitHistoryResponse
    {
        public string IdMedicalRecordEntry { get; set; }
        public DateTime VisitDateTime { get; set; }
        public List<string> Conditions { get; set; }
        public List<string> Treatments { get; set; }
        public List<string> Medications { get; set; }
        public string Notes { get; set; }
        public string PIC { get; set; }
    }
}
