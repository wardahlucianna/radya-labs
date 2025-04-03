using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using System;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate
{
    public class DetailCertificateTemplateResult : ItemValueVm
    {
        public DateTime? DateCreated { get; set; }
        public CodeWithIdVm AcademicYear { get; set; }
        public string TemplateName { get; set; }
        public string CertificateTitle { get; set; } 
        public string TemplateSubtitle { get; set; }
        public string StatusApproval { get; set; }
        public string Reason { get; set; }
        public string Background { get; set; }
        public CodeWithIdVm Signature1 { get; set; }
        public string Signature1As { get; set; }
        public CodeWithIdVm Signature2 { get; set; }
        public string Signature2As { get; set; }
        public bool IsUseBinusLogo { get; set; }
        public string LinkBinusLogo { get; set; }
        public string Description { get; set; }
        public bool CanApprove { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public string UserApproverName { get; set; }
    }
}