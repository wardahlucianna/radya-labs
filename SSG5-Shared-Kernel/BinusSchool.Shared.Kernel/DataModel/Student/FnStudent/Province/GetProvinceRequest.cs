using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Province
{
    public class GetProvinceRequest : CollectionRequest
    {
        public string IdProvince { get; set; }
        public string IdCountry { get; set; }
    }
}
