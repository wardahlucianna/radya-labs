using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalTappingSystem
{
    public class UpdateMedicalTappingSystemPatientStatusRequest
    {
        public string IdBinusian { get; set; }
        public int Status { get; set; }
        public string IdSchool { get; set; }
    }
}
