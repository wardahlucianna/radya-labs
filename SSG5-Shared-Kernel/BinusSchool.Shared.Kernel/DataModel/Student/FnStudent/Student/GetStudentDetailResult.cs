using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Information;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class GetStudentDetailResult : ItemValueVm
    {
        public NameInfoVm NameInfo { get; set; }
        public PersonalStudentInfoDetailVm PersonalInfoVm { get; set; }
        public IdInfoDetailVm IdInfo { get; set; }
        public BirthInfoDetailVm BirthInfo { get; set; }
        public ReligionInfoVm ReligionInfo { get; set; }
        public AddressInfoDetailVm AddressInfo { get; set; }
        public BankAndVAInfoDetailVm BankAndVAInfo { get; set; }
        public PreviousSchoolInfoVm PreviousSchoolInfo { get; set; }
        public CardInfoVm CardInfo { get; set; }
        public ContactInfoDetailVm ContactInfo { get; set; }
        public MedicalInfoVm MedicalInfo { get; set; }
        public List<OccupationInfoVm> OccupationInfo { get; set; }
        public AdditionalInfoVm AdditionalInfo { get; set; }
        public SpecialTreatmentInfoVm SpecialTreatmentInfo { get; set; }
        public AuditableResult Audit { get; set; }
        public IReadOnlyList<string> SiblingGroup { get; set; }
        public List<ParentGroupDetailVm> ParentGroup { get; set; }
        public ItemValueVm StudentStatus { get; set; }
    }

    public class ParentGroupDetailVm
    {
        public ParentGroupDetailVm(string id, string description)
        {
            Id = id;
            Description = description;
            IdEncryptedRjindael = "";
        }
        public string Id { get; set; }
        public string Description { get; set; }
        public string IdEncryptedRjindael { get; set; }
    }
   


    public class ContactInfoDetailVm
    {
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string MobilePhoneNumber2 { get; set; }
        public string MobilePhoneNumber3 { get; set; }
        public EmergencyContactInfoDetailVm EmergencyContactRole { get; set; }
        public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
    }
    public class EmergencyContactInfoDetailVm
    {
        public string IdParent { get; set; }
        public ItemValueVm EmergencyContact { get; set; }
        public string EmergencyContactName { get; set; }
        public string EmergencyContactNumber { get; set; }
        public string EmergencyEmail { get; set; }
    }
    public class BankAndVAInfoDetailVm
    {
        public ItemValueVm BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public string BankAccountRecipient { get; set; }
        public string SchoolVAName { get; set; }
        public string SchoolVANumber { get; set; }
    }
    public class PersonalStudentInfoDetailVm
    {
        public string Photo { get; set; }
        public string Grade { get; set; }
        public string Class { get; set; }
        public int IdStudentStatus { get; set; }
        public string StudentStatus { get; set; }
        public string SchoolLevel { get; set; }
        public int ChildNumber { get; set; }
        public int TotalChildInFamily { get; set; }
        public ItemValueVm IdChildStatus { get; set; }
    }

    public class BirthInfoDetailVm
    {
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        public ItemValueVm IdBirthStateProvince { get; set; }
        public ItemValueVm IdBirthCity { get; set; }
        public ItemValueVm IdNationality { get; set; }
        public ItemValueVm IdCountry { get; set; }
        public ItemValueVm IdBirthCountry { get; set; }
    }

    public class IdInfoDetailVm
    {

        public string IdSiblingGroup { get; set; }
        public string IdRegistrant { get; set; }
        public ItemValueVm IdSchool { get; set; }
        public string IdBinusian { get; set; }
        public string NISN { get; set; }
    }

    public class AddressInfoDetailVm
    {
        public ItemValueVm IdStayingWith { get; set; }
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
