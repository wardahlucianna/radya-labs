using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentHistory;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IFormDocumentHistory : IFnDocument
    {
        [Get("/document/doc-history")]
        Task<ApiErrorResult<IEnumerable<GetDocumentHistoryResult>>> GetDocumentHistories(GetDocumentHistoryRequest ParameterType);

        [Get("/document/doc-history/{id}")]
        Task<ApiErrorResult<GetDocumentHistoryDetailResult>> GetDocumentHistoryDetail(string id);
    }
}