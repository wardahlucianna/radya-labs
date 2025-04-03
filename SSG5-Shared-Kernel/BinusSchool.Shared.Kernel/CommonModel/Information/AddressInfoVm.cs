using System;
using System.Collections.Generic;

namespace BinusSchool.Common.Model.Information
{
    public class AddressInfoVm
    {
        public Int16 IdStayingWith { get; set; }
        public string ResidenceAddress { get; set; }
        public string HouseNumber { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public string VillageDistrict { get; set; }
        public string SubDistrict { get; set; }
        public string IdAddressCity { get; set; }
        public string IdAddressStateProvince { get; set; }
        public string IdAddressCountry { get; set; }
        public string PostalCode { get; set; }
        public decimal DistanceHomeToSchool { get; set; }
    }
}
