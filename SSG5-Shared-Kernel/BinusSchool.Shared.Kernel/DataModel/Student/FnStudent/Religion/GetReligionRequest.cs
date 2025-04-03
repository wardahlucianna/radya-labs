using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Religion
{
    public class GetReligionRequest : CollectionRequest
    {
        public string IdReligion { get; set; }
    }
}
