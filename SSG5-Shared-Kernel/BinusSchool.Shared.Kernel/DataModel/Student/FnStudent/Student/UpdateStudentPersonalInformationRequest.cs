using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class UpdateStudentPersonalInformationRequest
    {
        public string IdStudent { get; set; }
        public string IdBinusian { get; set; }
        public string NISN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        public string IdBirthCountry { get; set; }
        public string IdBirthCountryDesc { get; set; }
        public string IdBirthStateProvince { get; set; }
        public string IdBirthStateProvinceDesc { get; set; }
        public string IdBirthCity { get; set; }
        public string IdBirthCityDesc { get; set; }
        public string IdNationality { get; set; }
        public string IdNationalityDesc { get; set; }
        public string IdCountry { get; set; }
        public string IdCountryDesc { get; set; }
        public string FamilyCardNumber { get; set; }
        public string NIK { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportExpDate { get; set; }
        public string IdReligion { get; set; }
        public string IdReligionDesc { get; set; }
        public string IdReligionSubject { get; set; }
        public string IdReligionSubjectDesc { get; set; }
        public int ChildNumber { get; set; }
        public int TotalChildInFamily { get; set; }
        public string IdChildStatus { get; set; }
        public string IdChildStatusDesc { get; set; }
        public Int16 IsHavingKJP { get; set; }
        public int IsParentUpdate { get; set; }
        public Gender Gender { get; set; }
    }
}

