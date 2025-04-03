using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class SaveMedicalDocumentRequest
    {
        public string Id { get; set; }
        public string Mode { get; set; }
        public string IdDocument { get; set; }
        public string DocumentName { get; set; }
    }
}
