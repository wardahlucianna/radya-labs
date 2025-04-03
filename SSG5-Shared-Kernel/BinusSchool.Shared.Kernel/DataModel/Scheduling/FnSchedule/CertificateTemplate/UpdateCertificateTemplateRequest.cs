using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate
{
    public class UpdateCertificateTemplateRequest
    {
        public string Id { get; set; }
        public string IdAcademicYear { get; set; }
        public bool IsUseBinusLogo { get; set; }
        public string Name { get; set; }
        public string Title { get; set; } 
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public string Background { get; set; }
        public string Signature1 { get; set; }
        public string Signature1As { get; set; }
        public string? Signature2 { get; set; }
        public string? Signature2As { get; set; }
    }
}
