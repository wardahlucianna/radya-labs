using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MedicalSystem.MedicalData
{
    public class MedicalDataSchoolRequest : ISingleSchool, IReturnCollection
    {
        public string IdSchool { get; set; }
        public CollectionType Return { get; set; }
    }
}
