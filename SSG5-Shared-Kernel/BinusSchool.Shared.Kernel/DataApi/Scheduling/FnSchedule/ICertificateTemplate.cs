using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CertificateTemplate;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface ICertificateTemplate : IFnSchedule
    {
        [Get("/schedule/certificate-template")]
        Task<ApiErrorResult<IEnumerable<GetCertificateTemplateResult>>> GetCertificateTemplates(GetCertificateTemplateRequest query);

        [Get("/schedule/certificate-template-detail")]
        Task<ApiErrorResult<DetailCertificateTemplateResult>> GetCertificateTemplateDetail(DetailCertificateTemplateRequest query);

        [Post("/schedule/certificate-template")]
        Task<ApiErrorResult> AddCertificateTemplate([Body] AddCertificateTempRequest body);

        [Put("/schedule/certificate-template")]
        Task<ApiErrorResult> UpdateCertificateTemplate([Body] UpdateCertificateTemplateRequest body);

        [Delete("/schedule/certificate-template-delete")]
        Task<ApiErrorResult> DeleteCertificateTemplate([Body] DeleteCertificateTemplateRequest body);

        [Put("/schedule/certificate-template-approval")]
        Task<ApiErrorResult> SetCertificateTemplateApproval([Body] SetCertificateTemplateApprovalRequest body);

        [Get("/schedule/certificate-template-by-ay")]
        Task<ApiErrorResult<IEnumerable<GetCertificateTemplateByAYResult>>> GetCertificateTemplateByAY(GetCertificateTemplateByAYRequest query);

    }
}
