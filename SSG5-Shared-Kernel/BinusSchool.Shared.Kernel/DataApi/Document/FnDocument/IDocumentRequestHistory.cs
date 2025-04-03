using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestHistory;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IDocumentRequestHistory : IFnDocument
    {
        [Get("/document-request-history/get-list-document-request-year")]
        Task<ApiErrorResult<IEnumerable<GetListDocumentRequestYearResult>>> GetListDocumentRequestYear(GetListDocumentRequestYearRequest param);

        [Get("/document-request-history/get-document-request-history-by-student")]
        Task<ApiErrorResult<IEnumerable<GetDocumentRequestHistoryByStudentResult>>> GetDocumentRequestHistoryByStudent(GetDocumentRequestHistoryByStudentRequest param);

        [Delete("/document-request-history/cancel-document-request-by-parent")]
        Task<ApiErrorResult> CancelDocumentRequestByParent([Body] CancelDocumentRequestByParentRequest param);
    }
}
