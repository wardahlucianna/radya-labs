using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal
{
    public class AddUnivInformationManagementPortalApprovalRequest
    {
        public string IdUserApproval { get; set; }

        public string IdSchool { get; set; }

        public string IdUnivInformationManagementPortal { get; set; }

        public bool IsApproval { get; set; }

    }
}
