using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class UpdateStudentRequest
    {
        public string IdStudent { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdRegistrant { get; set; }
        public string IdSchool { get; set; }
        public string IdBinusian { get; set; }
        public string NISN { get; set; }
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        public string IdBirthCountry { get; set; }
        public string IdBirthStateProvince { get; set; }
        public string IdBirthCity { get; set; }
        public string IdNationality { get; set; }
        public string IdCountry { get; set; }
        public string FamilyCardNumber { get; set; }
        public string NIK { get; set; }
        public string KITASNumber { get; set; }
        public DateTime? KITASExpDate { get; set; }
        public string NSIBNumber { get; set; }
        public DateTime? NSIBExpDate { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportExpDate { get; set; }
        public string IdReligion { get; set; }
        public string IdReligionSubject { get; set; }
        public int ChildNumber { get; set; }
        public int TotalChildInFamily { get; set; }
        public string IdChildStatus { get; set; }
        public Int16 IsHavingKJP { get; set; }
        public Int16 IsSpecialTreatment { get; set; }
        public string NotesForSpecialTreatments { get; set; }
        public string IdBloodType { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string ResidenceAddress { get; set; }
        public string HouseNumber { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public Int16 IdStayingWith { get; set; }
        public string VillageDistrict { get; set; }
        public string SubDistrict { get; set; }
        public string IdAddressCity { get; set; }
        public string IdAddressStateProvince { get; set; }
        public string IdAddressCountry { get; set; }
        public string PostalCode { get; set; }
        public decimal DistanceHomeToSchool { get; set; }
        public string ResidencePhoneNumber { get; set; }
        public string MobilePhoneNumber1 { get; set; }
        public string MobilePhoneNumber2 { get; set; }
        //public string MobilePhoneNumber3 { get; set; }
        public string EmergencyContactRole { get; set; }
        public string BinusianEmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string FutureDream { get; set; }
        public string Hobby { get; set; }
        public Gender Gender { get; set; }
        public string SchoolVANumber { get; set; }
        public string SchoolVAName { get; set; }
        public int IdStudentStatus { get; set; }

    }
}
