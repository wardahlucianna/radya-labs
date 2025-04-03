using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal
{
    public class GetUnivInformationManagementPortalApprovalResult : ItemValueVm
    {
        public CodeWithIdVm FromSchool { get; set; }
        public string CreatedBy { get; set; }
        public string UnivercityName { get; set; }
        public string UnivercityWebsite { get; set; }
        public string ContactPerson { get; set; }
        public string IdUnivInformationManagementPortal { get; set; }

        public List<LogoUnivInformationManagementPortalApproval> Logo { get; set; }

        public class LogoUnivInformationManagementPortalApproval
        {
            public string Id { get; set; }
            public string Url { get; set; }
            public string OriginalFilename { get; set; }
            public string FileName { get; set; }
            public string FileType { get; set; }
            public int FileSize { get; set; }
        }
    }
}
