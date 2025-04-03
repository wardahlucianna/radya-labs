using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.City
{
    public class GetCityRequest : CollectionRequest
    {
        public string IdCity { get; set; }
        public string IdProvince { get; set; }
        public string IdCountry { get; set; }
    }
}
