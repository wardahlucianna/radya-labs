using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.Category;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IDocCategory : IFnDocument
    {
        [Get("/document/category")]
        Task<ApiErrorResult<IEnumerable<GetCategoryResult>>> GetCategories(GetCategoryRequest param);

        [Get("/document/category/{id}")]
        Task<ApiErrorResult<GetCategoryDetailResult>> GetCategoryDetail(string id);
    }
}
