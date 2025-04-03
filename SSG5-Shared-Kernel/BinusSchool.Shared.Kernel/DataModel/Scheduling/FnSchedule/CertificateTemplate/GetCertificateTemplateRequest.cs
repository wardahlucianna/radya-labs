using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate
{
    public class GetCertificateTemplateRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdSchool { get; set; }
        public string ApprovalStatus { get; set; }
        public string UserId { get; set; }

    }
}
