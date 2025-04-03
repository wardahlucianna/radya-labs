using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using System;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate
{
    public class GetCertificateTemplateByAYResult : ItemValueVm
    {
        public string AcademicYear { get; set; }
        public string TemplateName { get; set; }
    }
}