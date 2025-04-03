using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IDocumentRequestWorkflow : IFnDocument
    {
        [Post("/document-request-status-workflow/add-document-request-workflow")]
        Task<ApiErrorResult<AddDocumentRequestWorkflowResult>> AddDocumentRequestWorkflow([Body] AddDocumentRequestWorkflowRequest param);

        [Get("/document-request-status-workflow/get-document-request-status-workflow-list")]
        Task<ApiErrorResult<IEnumerable<GetDocumentRequestStatusWorkflowListResult>>> GetDocumentRequestStatusWorkflowList(GetDocumentRequestStatusWorkflowListRequest param);

        [Get("/document-request-status-workflow/get-date-business-days-by-start-date")]
        Task<ApiErrorResult<GetDateBusinessDaysByStartDateResult>> GetDateBusinessDaysByStartDate(GetDateBusinessDaysByStartDateRequest param);
    }
}
