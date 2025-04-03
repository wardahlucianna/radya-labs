using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.BNSReportSettings.ReportTemplate;
using BinusSchool.Data.Model.Scoring.FnScoring.BNSReportSettings.StaffSignature;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IBNSReportSettings : IFnScoring
    {
        #region Staff Signature
        [Get("/bns-report-settings/get-all-signature")]
        Task<ApiErrorResult<IEnumerable<GetAllSignatureResult>>> GetAllSignature(GetAllSignatureRequest query);

        [Post("/bns-report-settings/create-staff-signature")]
        Task<ApiErrorResult> CreateStaffSignature([Body] CreateStaffSignatureRequest param);

        [Get("/bns-report-settings/get-staff-list")]
        Task<ApiErrorResult<IEnumerable<GetStaffListResult>>> GetStaffList(GetStaffListRequest query);
        #endregion

        #region BNS Report Template
        [Get("/bns-report-settings/get-bns-report-template")]
        Task<ApiErrorResult<IEnumerable<GetBNSReportTemplateResult>>> GetBNSReportTemplate(GetBNSReportTemplateRequest query);

        [Get("/bns-report-settings/get-bns-report-template-detail")]
        Task<ApiErrorResult<GetBNSReportTemplateDetailResult>> GetBNSReportTemplateDetail(GetBNSReportTemplateDetailRequest query);

        [Post("/bns-report-settings/save-bns-report-template")]
        Task<ApiErrorResult<SaveBNSReportTemplateResult>> SaveBNSReportTemplate([Body] SaveBNSReportTemplateRequest param);

        [Delete("/bns-report-settings/delete-bns-report-template")]
        Task<ApiErrorResult> DeleteBNSReportTemplate([Body] DeleteBNSReportTemplateRequest param);

        [Get("/bns-report-settings/get-bns-report-template-reference")]
        Task<ApiErrorResult<IEnumerable<GetBNSReportTemplateReferenceResult>>> GetBNSReportTemplateReference(GetBNSReportTemplateReferenceRequest query);

        [Put("/bns-report-settings/update-bns-report-template-current-status")]
        Task<ApiErrorResult> UpdateBNSReportTemplateCurrentStatus([Body] UpdateBNSReportTemplateCurrentStatusRequest param);

        [Get("/bns-report-settings/get-paper-kind-bns-report-template")]
        Task<ApiErrorResult<IEnumerable<GetPaperKindBNSReportTemplateResult>>> GetPaperKindBNSReportTemplate(GetPaperKindBNSReportTemplateRequest query);

        [Post("/bns-report-settings/update-master-toc-report")]
        Task <ApiErrorResult> UpdateMasterTOCReport([Body] UpdateMasterTOCReportRequest param);

        [Post("/bns-report-settings/update-bns-report-template-reference")]
        Task<ApiErrorResult> UpdateBNSReportTemplateReference([Body] UpdateBNSReportTemplateReferenceRequest param);
        #endregion
    }
}
