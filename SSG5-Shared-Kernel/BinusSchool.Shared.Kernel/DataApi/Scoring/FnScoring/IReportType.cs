using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportType;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IReportType : IFnScoring
    {
        [Get("/BnsReport/ReportType")]
        Task<ApiErrorResult<IEnumerable<GetReportTypeResult>>> GetReportType(GetReportTypeRequest query);

        [Get("/BnsReport/ReportType/{id}")]
        Task<ApiErrorResult<GetReportTypeDetailResult>> GetReportTypeDetail(string id);

        [Post("/BnsReport/ReportType")]
        Task<ApiErrorResult> UpdateReportType([Body] UpdateReportTypeRequest query);

        [Delete("/BnsReport/ReportType")]
        Task<ApiErrorResult> DeleteReportType([Body] IEnumerable<string> ids);
    }
}
