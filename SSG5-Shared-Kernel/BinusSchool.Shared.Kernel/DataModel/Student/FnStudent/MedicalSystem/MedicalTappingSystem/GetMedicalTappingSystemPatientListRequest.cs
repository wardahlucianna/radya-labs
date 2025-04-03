using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalTappingSystem
{
    public class GetMedicalTappingSystemPatientListRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
