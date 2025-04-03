using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparationApproval;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface ITextbookPreparationApproval : IFnSchool
    {
        [Get("/school/textbook-preparation/approval")]
        Task<ApiErrorResult<List<GetTextbookPreparationApprovalResult>>> GetTextbookPreparationApproval(GetTextbookPreparationApprovalRequest param);

        [Post("/school/textbook-preparation/approval")]
        Task<ApiErrorResult> TextbookPreparationApproval([Body] TextbookPreparationApprovalRequest body);

        [Get("/school/textbook-preparation/approval-download")]
        Task<HttpResponseMessage> DownloadTextbookPreparationApproval(GetTextbookPreparationApprovalRequest param);

        
    }
}
