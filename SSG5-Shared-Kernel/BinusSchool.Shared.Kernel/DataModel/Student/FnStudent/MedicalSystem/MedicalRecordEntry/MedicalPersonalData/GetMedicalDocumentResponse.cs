using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalRecordEntry.MedicalPersonalData
{
    public class GetMedicalDocumentResponse
    {
        public string IdMedicalDocument { get; set; }
        public DateTime? UploadTime { get; set; }
        public string UploadBy { get; set; }
        public string MedicalDocumentName { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
    }
}
