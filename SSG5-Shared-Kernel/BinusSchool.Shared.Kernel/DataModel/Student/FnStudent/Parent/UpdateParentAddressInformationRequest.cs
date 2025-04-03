using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Parent
{
    public class UpdateParentAddressInformationRequest
    {
        public string IdParent { get; set; }
        public string IdStudent { get; set; }
        public string IdParentRole { get; set; }
        public string ResidenceAddress { get; set; }
        public string HouseNumber { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public string VillageDistrict { get; set; }
        public string SubDistrict { get; set; }
        public string IdAddressCity { get; set; }
        public string IdAddressCityDesc { get; set; }
        public string IdAddressStateProvince { get; set; }
        public string IdAddressStateProvinceDesc { get; set; }
        public string IdAddressCountry { get; set; }
        public string IdAddressCountryDesc { get; set; }
        public string PostalCode { get; set; }
        public int IsParentUpdate { get; set; }
    }
}
