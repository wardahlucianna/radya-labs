using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Information;

namespace BinusSchool.Data.Model.Student.FnStudent.Parent
{
    public class GetParentDetailResult : ItemValueVm
    {
        public NameInfoVm NameInfo { get; set; }
        public PersonalParentInfoVm PersonalInfoVm { get; set; }
        public MedicalInfoVm MedicalInfo { get; set; }
        public IdInfoVm IdInfo { get; set; }
        public BirthInfoDetailVm BirthInfo { get; set; }
        public ReligionInfoVm ReligionInfo { get; set; }
        public AddressInfoDetailVm AddressInfo { get; set; }
        public CardInfoVm CardInfo { get; set; }
        public ContactInfoVm ContactInfo { get; set; }
        public OccupationInfoVm OccupationInfo { get; set; }
        public AuditableResult Audit { get; set; }
    }
    public class BirthInfoDetailVm
    {
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        public ItemValueVm IdNationality { get; set; }
        public ItemValueVm IdCountry { get; set; }
    }

    public class AddressInfoDetailVm
    {
        public Int16 IdStayingWith { get; set; }
        public string ResidenceAddress { get; set; }
        public string HouseNumber { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public string VillageDistrict { get; set; }
        public string SubDistrict { get; set; }
        public ItemValueVm IdAddressCity { get; set; }
        public ItemValueVm IdAddressStateProvince { get; set; }
        public ItemValueVm IdAddressCountry { get; set; }
        public string PostalCode { get; set; }
        public decimal DistanceHomeToSchool { get; set; }
    }
}
