using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using System;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate
{
    public class GetCertificateTemplateResult : ItemValueVm
    {
        public string AcademicYear { get; set; }
        public string TemplateName { get; set; }
        public string CertificateTitle { get; set; } 
        public string TemplateSubtitle { get; set; }
        public string StatusApproval { get; set; }
        public bool CanApprove { get; set; }
        public bool CanDelete { get; set; }
    }
}