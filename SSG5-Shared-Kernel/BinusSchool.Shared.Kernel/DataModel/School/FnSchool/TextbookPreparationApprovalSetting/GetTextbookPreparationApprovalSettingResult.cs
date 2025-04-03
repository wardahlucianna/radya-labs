using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApprovalSetting
{
    public class GetTextbookPreparationApprovalSettingResult : CodeWithIdVm
    {
        public string IdUser { get; set; }
        public string UserDisplayName { get; set; }
        public string IdRole { get; set; }
        public string IdTeacherPosition { get; set; }
        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public int ApproverTo { get; set; }

    }
}
