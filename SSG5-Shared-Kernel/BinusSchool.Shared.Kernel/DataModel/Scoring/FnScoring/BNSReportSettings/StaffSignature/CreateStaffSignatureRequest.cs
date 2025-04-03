using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.BNSReportSettings.StaffSignature
{
    public class CreateStaffSignatureRequest
    {
        public string IdTeacherSignature { get; set; }
        public string IdBinusian { get; set; }
        public string Url { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
    }
}
