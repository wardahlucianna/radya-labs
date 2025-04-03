using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.District
{
    public class GetDistrictResult : CollectionRequest
    {
        public string IdDistrict { get; set; }
        public string DistrictName { get; set; }
    }
}