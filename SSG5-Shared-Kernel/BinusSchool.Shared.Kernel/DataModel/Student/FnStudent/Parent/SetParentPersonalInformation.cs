using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Parent
{
    public class SetParentPersonalInformation
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdBinusian { get; set; }
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        public string FamilyCardNumber { get; set; }
        public string NIK { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportExpDate { get; set; }
        public string KITASNumber { get; set; }
        public DateTime? KITASExpDate { get; set; }
        public string ParentNameForCertificate { get; set; }
        public ItemValueVm BinusianStatus { get; set; }
        public ItemValueVm AliveStatus { get; set; }
        public ItemValueVm IdLastEducationLevel { get; set; }
        //public ItemValueVm IdParentRole { get; set; }
        public ItemValueVm IdNationality { get; set; }
        public ItemValueVm IdCountry { get; set; }
        public ItemValueVm IdReligion { get; set; }
        public ItemValueVm IdParentRelationship { get; set; }
    }
}
