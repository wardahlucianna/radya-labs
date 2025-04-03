using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad;
using Refit;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface INonTeachingLoad : IFnAssignment
    {
        [Get("/assignment/non-teaching-load")]
        Task<ApiErrorResult<IEnumerable<GetNonTeachLoadResult>>> GetNonTeachingLoads(GetNonTeachLoadRequest query);
        [Get("/assignment/non-teaching-load-copy")]
        Task<ApiErrorResult<IEnumerable<GetCopyNonTeachingLoadResult>>> GetCopyNonTeachingLoads(GetCopyNonTeachingLoadRequest query);
        [Get("/assignment/non-teaching-load/{id}")]
        Task<ApiErrorResult<GetNonTeachLoadDetailResult>> GetNonTeachingLoadDetail(string id);

        [Get("/assignment/non-teaching-load-previous")]
        Task<ApiErrorResult<IEnumerable<GetRecentHierarchy>>> GetPreviousNonTeachingLoad(GetPreviousHierarchyNonTeachingLoadRequest query);

        [Post("/assignment/non-teaching-load")]
        Task<ApiErrorResult> AddNonTeachingLoad([Body] AddNonTeachLoadRequest body);

        [Put("/assignment/non-teaching-load")]
        Task<ApiErrorResult> UpdateNonTeachingLoad([Body] UpdateNonTeachLoadRequest body);

        [Delete("/assignment/non-teaching-load")]
        Task<ApiErrorResult> DeleteNonTeachingLoad([Body] IEnumerable<string> ids);

        [Post("/assignment/non-teaching-load/copy")]
        Task<ApiErrorResult> CopyNonTeachingLoad([Body] CopyNonTeachingLoadRequest body);
    }
}
