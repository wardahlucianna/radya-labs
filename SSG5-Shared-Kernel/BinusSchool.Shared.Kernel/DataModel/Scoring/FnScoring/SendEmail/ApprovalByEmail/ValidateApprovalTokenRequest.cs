using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SendEmail.ApprovalByEmail
{
    public class ValidateApprovalTokenRequest
    {
        public string ActionKey { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
    }
}
