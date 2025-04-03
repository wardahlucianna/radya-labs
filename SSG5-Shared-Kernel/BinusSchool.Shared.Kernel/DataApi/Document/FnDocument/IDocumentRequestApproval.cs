using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApproval;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IDocumentRequestApproval : IFnDocument
    {
        [Post("/document-request-approval/save-document-request-approval")]
        Task<ApiErrorResult> SaveDocumentRequestApproval([Body] SaveDocumentRequestApprovalRequest param);
    }
}
