using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate
{
    public class DetailCertificateTemplateRequest : CollectionSchoolRequest
    {
        public string IdCertificateTemplate { get; set; }
        public string UserId { get; set; }

    }
}
