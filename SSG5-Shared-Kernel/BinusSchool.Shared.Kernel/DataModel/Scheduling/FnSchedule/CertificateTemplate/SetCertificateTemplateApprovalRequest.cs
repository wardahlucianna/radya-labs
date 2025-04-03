using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate
{
    public class SetCertificateTemplateApprovalRequest
    {
        public string Id { get; set; }
        //id certificate template
        public string UserId { get; set; }
        public bool IsApproved  { get; set; }
        public string? Reason { get; set; }
    }
}