using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.SendEmail.ApprovalByEmail
{
    public class CreateApprovalTokenResult
    {
        public string ApproveActionKey { get; set; }
        public string RejectActionKey { get; set; }
        public DateTime ExpiredDate { get; set; }
    }
}
