using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal
{
    public class UpdateUnivInformationManagementPortalRequest
    {
        public string IdSchool { get; set; }
        public string IdUnivInformationManagementPortal { get; set; }
        public string UnivercityName { get; set; }
        public string UnivercityDescription { get; set; }
        public string UnivercityWebsite { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public bool IsSquareLogo { get; set; }
        public bool IsShare { get; set; }
        public List<FactSheetUnivInformationManagementPortal> FactSheet { get; set; }
        public List<LogoUnivInformationManagementPortal> Logo { get; set; }
    }
}
