using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.TransferStudentData
{
    public class TransferMasterDistrictRequest
    {
        public IEnumerable<District> DistrictList { get; set; }     
    }
     public class District{
        public string IdDistrict { get; set; }
        public string DistrictName { get; set; }
    }
}
