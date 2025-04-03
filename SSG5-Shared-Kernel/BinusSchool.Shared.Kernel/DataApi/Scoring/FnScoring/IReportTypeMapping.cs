using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportTypeMapping;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IReportTypeMapping : IFnScoring
    {
        [Get("/BnsReport/ReportTypeMapping/grade-mapped")]
        Task<ApiErrorResult<IEnumerable<GetReportTypeGradeMappedResult>>> GetReportTypeGradeMapped(GetReportTypeGradeMappedRequest query);

        [Put("/BnsReport/ReportTypeMapping/update-mapping")]
        Task<ApiErrorResult> UpdateReportTypeMapping([Body] UpdateReportTypeMappingRequest query);

        [Get("/BnsReport/ReportTypeMapping/get-detail")]
        Task<ApiErrorResult<GetReportTypeMappingDetailResult>> GetReportTypeMappingDetail(GetReportTypeMappingDetailRequest query);

        [Delete("/BnsReport/ReportTypeMapping/delete-mapping")]
        Task<ApiErrorResult> DeleteReportTypeMapping([Body] DeleteReportTypeMappingRequest query);

        [Get("/BnsReport/ReportTypeMapping/getlist-reporttype-mapping")]
        Task<ApiErrorResult<IEnumerable<GetListReportTypeMappingResult>>> GetListReportTypeMapping(GetListReportTypeMappingRequest query);

        [Post("/BnsReport/ReportTypeMapping/transfer-reporttype-mapping")]
        Task<ApiErrorResult<TransferReportTypeMappingResult>> TransferReportTypeMapping([Body] TransferReportTypeMappingRequest query);
    }
}
