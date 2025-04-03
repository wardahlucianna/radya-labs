using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.Parent
{
    public class GetParentRequest : CollectionSchoolRequest
    {
        //public string IdStudent { get; set; }
        public string IdParentRole { get; set; }
        public string IdReligion { get; set; }
        public string IdLastEducationLevel { get; set; }
        public string IdNationality { get; set; }
        public string IdCountry { get; set; }
        public string IdAddressCity { get; set; }
        public string IdAddressStateProvince { get; set; }
        public string IdAddressCountry { get; set; }
        public string IdOccupationType { get; set; }
        public string IdParentSalaryGroup { get; set; }
        public Gender Gender { get; set; }
    }
}
