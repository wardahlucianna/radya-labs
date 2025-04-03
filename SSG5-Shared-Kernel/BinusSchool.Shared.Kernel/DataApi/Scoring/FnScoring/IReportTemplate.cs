using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportTemplate;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IReportTemplate : IFnScoring
    {
        [Get("/BnsReport/ReportTemplate")]
        Task<ApiErrorResult<IEnumerable<GetReportTemplateResult>>> GetReportTemplate(GetReportTemplateRequest query);

        [Get("/BnsReport/ReportTemplate/{id}")]
        Task<ApiErrorResult<GetReportTemplateDetailResult>> GetReportTemplateDetail(string id);

        [Get("/BnsReport/ReportTemplate/GetReportTemplateDetailByCode")]
        Task<ApiErrorResult<GetReportTemplateDetailByCodeResult>> GetReportTemplateDetailByCode(GetReportTemplateDetailByCodeRequest query);
    }
}
