using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class UpdateStudentAddressInformationRequest
    {
        public string IdStudent { get; set; }
        public string ResidenceAddress { get; set; }
        public string HouseNumber { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public string IdStayingWith { get; set; }
        public string IdStayingWithDesc { get; set; }
        public string VillageDistrict { get; set; }
        public string SubDistrict { get; set; }
        public string IdAddressCity { get; set; }
        public string IdAddressCityDesc { get; set; }
        public string IdAddressStateProvince { get; set; }
        public string IdAddressStateProvinceDesc { get; set; }
        public string IdAddressCountry { get; set; }
        public string IdAddressCountryDesc { get; set; }
        public string PostalCode { get; set; }
        public decimal DistanceHomeToSchool { get; set; }
        public int IsParentUpdate { get; set; }
    }
}
