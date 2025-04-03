using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Parent
{
    public class UpdateParentPersonalInformationRequest
    {
        public string IdParent { get; set; }
        public string IdStudent { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string POB { get; set; }
        public DateTime? DOB { get; set; }
        public string IdParentRole { get; set; }
        public Int16 AliveStatus { get; set; }
        public string AliveStatusDesc { get; set; }
        public string IdReligion { get; set; }
        public string IdReligionDesc { get; set; }
        public int? IdLastEducationLevel { get; set; }
        public string IdLastEducationLevelDesc { get; set; }
        public string IdNationality { get; set; }
        public string IdNationalityDesc { get; set; }
        public string IdCountry { get; set; }
        public string IdCountryDesc { get; set; }
        public string FamilyCardNumber { get; set; }
        public string NIK { get; set; }
        public string PassportNumber { get; set; }
        public DateTime? PassportExpDate { get; set; }
        public string KITASNumber { get; set; }
        public DateTime? KITASExpDate { get; set; }
        public Int16 BinusianStatus { get; set; }
        public string BinusianStatusDesc { get; set; }
        public string IdBinusian { get; set; }
        public string ParentNameForCertificate { get; set; }
        public int? IdParentRelationship { get; set; }
        public string IdParentRelationshipDesc { get; set; }
        public int IsParentUpdate { get; set; }

    }
}
