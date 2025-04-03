using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Common.Constants
{
    public class StatusConstant
    {
        public const string Draft = "Draft";
        public const string NeedApproval = "Need Approval";
        public const string Approve = "Approved";
        public const string Rejected = "Need Revision";

        public string[] All = new string[] { Draft, NeedApproval, Approve,Rejected };
    }


}
