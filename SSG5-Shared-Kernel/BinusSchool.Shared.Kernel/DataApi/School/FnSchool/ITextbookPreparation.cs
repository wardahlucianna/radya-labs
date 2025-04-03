using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparation;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface ITextbookPreparation : IFnSchool
    {
        [Get("/school/textbook-preparation/entry")]
        Task<ApiErrorResult<GetTextbookPreparationResult>> GetTextbookPreparation(GetTextbookPreparationRequest param);

        [Get("/school/textbook-preparation/entry-subject")]
        Task<ApiErrorResult<IEnumerable<NameValueVm>>> GetTextbookPreparationSubject(GetTextbookPreparationSubjectRequest param);

        [Post("/school/textbook-preparation/entry")]
        Task<ApiErrorResult> AddTextbookPreparation([Body] AddTextbookPreparationRequest body);

        [Get("/school/textbook-preparation/entry/{id}")]
        Task<ApiErrorResult<DetailTextbookPreparationResult>> DetailTextbookPreparation(string id);

        [Put("/school/textbook-preparation/entry")]
        Task<ApiErrorResult> UpdateTextbookPreparation([Body] UpdateTextbookPreparationRequest body);

        [Delete("/school/textbook-preparation/entry")]
        Task<ApiErrorResult> DeleteTextbookPreparation([Body] IEnumerable<string> body);

        [Multipart]
        [Post("/school/textbook-preparation/entry-upload")]
        Task<ApiErrorResult<GetUploadTextbookPreparationResult>> GetUploadTextbookPreparation(StreamPart stream);

        [Put("/school/textbook-preparation/entry-upload")]
        Task<ApiErrorResult> AddUploadTextbookPreparation([Body] AddUploadTextbookPreparationRequest body);

        [Get("/school/textbook-preparation/entry-download")]
        Task<HttpResponseMessage> DownloadTextbookPreparation(DownloadTextbookPreparationRequest param);
    }
}
