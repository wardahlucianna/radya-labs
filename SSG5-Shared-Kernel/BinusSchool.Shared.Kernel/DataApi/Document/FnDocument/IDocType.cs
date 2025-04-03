using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IDocType : IFnDocument
    {
        [Get("/document/type")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetTypes(CollectionRequest param);
    }
}
