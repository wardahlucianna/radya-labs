using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Level;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface ILevel : IFnSchool
    {
        [Get("/school/level")]
        Task<ApiErrorResult<IEnumerable<GetLevelResult>>> GetLevels(GetLevelRequest query);

        [Get("/school/level/{id}")]
        Task<ApiErrorResult<GetLevelDetailResult>> GetLevelDetail(string id);

        [Post("/school/level")]
        Task<ApiErrorResult> AddLevel([Body] AddLevelRequest body);

        [Put("/school/level")]
        Task<ApiErrorResult> UpdateLevel([Body] UpdateLevelRequest body);

        [Delete("/school/level")]
        Task<ApiErrorResult> DeleteLevel([Body] IEnumerable<string> ids);

        [Get("/school/level-acadyear")]
        Task<ApiErrorResult<IEnumerable<GetLevelResult>>> GetLevelsByAcadYear(GetLevelRequest query);
        
        [Get("/school/level-code")]
        Task<ApiErrorResult<IEnumerable<GetLevelCodeResult>>> GetLevelCodeList(GetLevelCodeRequest query);
    }
}
