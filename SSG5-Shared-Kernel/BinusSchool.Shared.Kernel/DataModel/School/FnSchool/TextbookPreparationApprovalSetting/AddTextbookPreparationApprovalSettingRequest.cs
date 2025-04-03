using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApprovalSetting
{
    public class AddTextbookPreparationApprovalSettingRequest
    {
        public string IdUser { get; set; }
        public string IdSchool { get; set; }
        public List<TextbookPreparationApprovalSettings> TextbookPreparationApprovalSetting { get; set; }
    }

    public class TextbookPreparationApprovalSettings
    {
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public string IdUser { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public int ApproverTo { get; set; }
    }
}
