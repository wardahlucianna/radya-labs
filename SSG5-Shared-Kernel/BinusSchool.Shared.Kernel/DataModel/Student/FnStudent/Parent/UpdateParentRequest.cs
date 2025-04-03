using System;

namespace BinusSchool.Data.Model.Student.FnStudent.Parent
{
    public class UpdateParentRequest
    {
        public string IdParent { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        public string IdParentRole { get; set; }
        public Int16 AliveStatus { get; set; }
        public string IdReligion { get; set; }
        public int? IdLastEducationLevel { get; set; } //belom mapped
        public string IdNationality { get; set; }
        public string IdCountry { get; set; }
        public string FamilyCardNumber { get; set; }
        public string NIK { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportExpDate { get; set; }
        public string KITASNumber { get; set; }
        public DateTime? KITASExpDate { get; set; }
        public Int16 BinusianStatus { get; set; }
        public string IdBinusian { get; set; }
        public string ParentNameForCertificate { get; set; }
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
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string MobilePhoneNumber2 { get; set; }
        public string MobilePhoneNumber3 { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string WorkEmailAddress { get; set; }
        public string IdOccupationType { get; set; }
        public string OccupationPosition { get; set; }
        public string CompanyName { get; set; }
        public int? IdParentSalaryGroup { get; set; }
        public string IdGender { get; set; }
        public int? IdParentRelationship { get; set; }
    }
}
