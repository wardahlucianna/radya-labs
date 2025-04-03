using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scoring.FnScoring.BNSReportSettings.StaffSignature
{
    public class GetStaffListRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public bool NotHideStaffSignature { get; set; }
    }
}
