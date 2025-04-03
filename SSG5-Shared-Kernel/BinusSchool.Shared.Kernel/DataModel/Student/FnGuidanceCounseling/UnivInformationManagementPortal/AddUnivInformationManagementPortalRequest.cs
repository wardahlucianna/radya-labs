using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal
{
    public class AddUnivInformationManagementPortalRequest
    {
        public string IdSchool { get; set; }
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
    public class FactSheetUnivInformationManagementPortal
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
    }

    public class LogoUnivInformationManagementPortal
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string OriginalFilename { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FileSize { get; set; }
    }
}
