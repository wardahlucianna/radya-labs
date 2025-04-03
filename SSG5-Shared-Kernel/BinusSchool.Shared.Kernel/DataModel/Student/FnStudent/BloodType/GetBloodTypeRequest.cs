using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.BloodType
{
    public class GetBloodTypeRequest : CollectionRequest
    {
        public string IdBloodType { get; set; }
    }
}
