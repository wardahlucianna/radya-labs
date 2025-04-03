using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent
{
    public class GetDownloadStudentCertificateResult : CodeWithIdVm
    {
        public string TemplateName { get; set; }
        public string CertificateTitle { get; set; } 
        public string TemplateSubtitle { get; set; }
        //public string Description { get; set; }
        public string StudentName { get; set; }
        public string AwardName { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string SchoolName { get; set; }
        public string Background { get; set; }
        public CodeWithIdVm Signature1 { get; set; }
        public string Signature1As { get; set; }
        public CodeWithIdVm Signature2 { get; set; }
        public string Signature2As { get; set; }
        public bool IsUseBinusLogo { get; set; }
        public string LinkSchoolBinusLogo { get; set; }
        public string UrlCertificate {get;set;}
        public string OriginalFilename { get; set; }
    }
}
