using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Division;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IDivision : IFnSchool
    {
        [Get("/school/dision")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetDivisions(CollectionSchoolRequest query);

        [Get("/school/dision/{id}")]
        Task<ApiErrorResult<DetailResult2>> GetDivisionDetail(string id);

        [Post("/school/dision")]
        Task<ApiErrorResult> AddDivision([Body] AddDivisionRequest body);

        [Put("/school/dision")]
        Task<ApiErrorResult> UpdateDivision([Body] UpdateDivisionRequest body);

        [Delete("/school/dision")]
        Task<ApiErrorResult> DeleteDivision([Body] IEnumerable<string> ids);

        [Get("/school/dision-combination")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetDivisionCombinations(GetDivisionCombinationRequest query);
    }
}
