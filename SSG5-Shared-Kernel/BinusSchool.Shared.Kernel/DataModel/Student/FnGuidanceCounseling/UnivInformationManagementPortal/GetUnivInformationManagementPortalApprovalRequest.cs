using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal
{
    public class GetUnivInformationManagementPortalApprovalRequest : CollectionRequest
    {
        public string IdSchool { get; set; }

        public string IdFromSchool { get; set; }

        //public ApprovalStatus Action { get; set; }
    }
}
