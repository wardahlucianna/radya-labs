using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Student
{
    public class SetStudentPersonalInformation
    {
        //public string IdBinusian { get; set; }
        public string NISN { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        public ItemValueVm IdBirthCountry { get; set; }
        public ItemValueVm IdBirthStateProvince { get; set; }
        public ItemValueVm IdBirthCity { get; set; }
        public ItemValueVm IdNationality { get; set; }
        public ItemValueVm IdCountry { get; set; }
        public string FamilyCardNumber { get; set; }
        public string NIK { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportExpDate { get; set; }
        public ItemValueVm IdReligion { get; set; }
        public ItemValueVm IdReligionSubject { get; set; }
        public int ChildNumber { get; set; }
        public int TotalChildInFamily { get; set; }
        public ItemValueVm IdChildStatus { get; set; }
        public Int16 IsHavingKJP { get; set; }
        public Gender Gender { get; set; }
    }
}
