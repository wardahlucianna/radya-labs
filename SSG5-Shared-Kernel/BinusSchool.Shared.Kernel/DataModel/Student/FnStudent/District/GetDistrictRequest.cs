using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.District
{
    public class GetDistrictRequest : CollectionRequest
    {
        public string IdDistrict { get; set; }
    }
}
