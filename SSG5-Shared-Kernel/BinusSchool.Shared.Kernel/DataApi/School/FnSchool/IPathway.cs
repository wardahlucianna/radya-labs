using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IPathway : IFnSchool
    {
        [Get("/school/pathway")]
        Task<ApiErrorResult<IEnumerable<GetPathwayResult>>> GetPathways(GetPathwayRequest query);

        [Get("/school/pathway/{id}")]
        Task<ApiErrorResult<GetPathwayDetailResult>> GetPathwayDetail(string id);

        [Post("/school/pathway")]
        Task<ApiErrorResult> AddPathway([Body] AddPathwayRequest body);

        [Put("/school/pathway")]
        Task<ApiErrorResult> UpdatePathway([Body] UpdatePathwayRequest body);

        [Delete("/school/pathway")]
        Task<ApiErrorResult> DeletePathway([Body] IEnumerable<string> ids);

        [Post("/school/copy-pathway")]
        Task<ApiErrorResult> CopyPathway([Body] CopyPathwayRequest body);

        [Get("/school/pathway-grade")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetPathwaysByGradePathway(GetPathwayGradeRequest query);

        [Get("/school/summary-pathway-grade")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetSummaryPathwaysByGradePathway(GetPathwayGradeRequest query);
    }
}
