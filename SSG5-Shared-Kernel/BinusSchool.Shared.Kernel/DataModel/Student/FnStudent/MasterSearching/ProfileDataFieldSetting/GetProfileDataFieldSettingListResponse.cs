using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterSearching.ProfileDataFieldSetting
{
    public class GetProfileDataFieldSettingListResponse
    {
        public string IdBinusian { get; set; }
        public List<GetProfileDataFieldSettingListResponse_FieldGroup> FieldGroup { get; set; }
    }

    public class GetProfileDataFieldSettingListResponse_FieldGroup
    {
        public string IdProfileDataFieldGroup { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public List<GetProfileDataFieldSettingListResponse_FieldGroup_Field> Field { get; set; }
    }

    public class GetProfileDataFieldSettingListResponse_FieldGroup_Field
    {
        public string IdProfileDataField { get; set; }
        public string AliasName { get; set; }
        public string Description { get; set; }
        public bool IsChecked { get; set; }
    }
}
