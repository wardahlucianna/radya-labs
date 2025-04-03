using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class SetStudentAddressInformation
    {
        public string ResidenceAddress { get; set; }
        public string HouseNumber { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public ItemValueVm IdStayingWith { get; set; }
        public string VillageDistrict { get; set; }
        public string SubDistrict { get; set; }
        public ItemValueVm IdAddressCity { get; set; }
        public ItemValueVm IdAddressStateProvince { get; set; }
        public ItemValueVm IdAddressCountry { get; set; }
        public string PostalCode { get; set; }
        public decimal DistanceHomeToSchool { get; set; }
    }
}
