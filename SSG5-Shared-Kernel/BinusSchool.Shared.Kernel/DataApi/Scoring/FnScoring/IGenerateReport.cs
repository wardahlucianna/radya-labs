using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.GenerateReport;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface  IGenerateReport : IFnScoring
    {
        [Get("/generate-report/student-homeroom-report")]
        Task<ApiErrorResult<GetStudentGenerateReportResult>> GetStudentGenerateReport(GetStudentGenerateReportRequest query);

        [Get("/generate-report/get-student-for-filter-generate-report")]
        Task<ApiErrorResult<GetStudentForFilterGenerateReportResult>> GetStudentForFilterGenerateReport(GetStudentForFilterGenerateReportRequest query);

        [Post("/generate-report/update-student-generate-process")]
        Task<ApiErrorResult> UpdateStudentGenerateReportProcess([Body] AddStudentGenerateReportProcessRequest body);

        [Post("/generate-report/generate-student-report")]
        Task<ApiErrorResult<GenerateStudentReportResult>> GenerateStudentReport([Body] GenerateStudentReportRequest body);

        [Post("/generate-report/sync-student-generate-process")]
        Task<ApiErrorResult> SyncStudentGenerateReport([Body] SyncStudentGenerateReportRequest body);

        [Post("/generate-report/add-toc-report")]
        Task<ApiErrorResult> AddTOCReport([Body] AddTOCReportRequest body);
    }
}
