using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.Document;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IFormDocument : IFnDocument
    {
        [Get("/document/doc")]
        Task<ApiErrorResult<IEnumerable<GetDocumentResult>>> GetDocuments(GetDocumentRequest param);

        [Get("/document/doc/{id}")]
        Task<ApiErrorResult<GetDocumentDetailResult>> GetDocumentDetail(string id);

        [Get("/document/doc-value")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetDocumentValue(SelectDocumentValueRequest req);

        [Put("/document/doc")]
        Task<ApiErrorResult> UpdateDocument([Body] UpdateDocumentRequest body);

        [Put("/document/doc-approval")]
        Task<ApiErrorResult> ApprovalDocument([Body] ApprovalDocumentRequest body);
    }
}