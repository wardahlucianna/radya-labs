using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scoring.FnScoring;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByHomeroom;
using BinusSchool.Shared.Kernel.DataModel.Scoring.FnScoring.StudentReflection;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataApi.Scoring.FnScoring
{
    public interface IStudentReflection : IFnScoring
    {
        [Get("/studentreflection/entry-period")]
        Task<ApiErrorResult<GetStudentReflectionEntryPeriodResult>> GetStudentReflectionEntryPeriod(GetStudentReflectionEntryPeriodRequest query);

        [Get("/studentreflection/detail")]
        Task<ApiErrorResult<GetStudentReflectionResult>> GetStudentReflection(GetStudentReflectionRequest query);

        [Post("/studentreflection/save-update-student-reflection")]
        Task<ApiErrorResult<SaveUpdateStudentReflectionResult>> SaveUpdateStudentReflection([Body] SaveUpdateStudentReflectionRequest body);

        [Get("/studentreflection/review")]
        Task<ApiErrorResult<GetReflectionListResult>> GetStudentReflectionReview(GetReflectionListRequest query);

        [Post("/studentreflection/update-reflection-status")]
        Task<HttpResponseMessage> UpdateReflectionStatus([Body] UpdateReflectionStatusRequest body);

        [Post("/studentreflection/export-excel-reflection")]
        Task<HttpResponseMessage> ExportExcelReflection([Body] GetReflectionListResult body);
    }
}
