using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MasterSearching.ProfileDataFieldSetting
{
    public class SaveProfileDataFieldSettingRequest
    {
        public string IdBinusian { get; set; }
        public List<string> IdProfileDataField { get; set; }
    }
}
