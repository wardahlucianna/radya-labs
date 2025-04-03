using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterSearching
{
    public class GetFieldDataListResult
    {
        public string IdProfileDataFieldGroup { get; set; }
        public string GroupName { get; set; }
        public List<GetProfileDataField> ListField { get; set; }
    }

    public class GetProfileDataField
    {
        public string IdProfileDataField { get; set; }
        public string ProfileDataFieldName { get; set; }
        public string AliasName { get; set; }
        public int OrderNumber { get; set; }
    }
}
