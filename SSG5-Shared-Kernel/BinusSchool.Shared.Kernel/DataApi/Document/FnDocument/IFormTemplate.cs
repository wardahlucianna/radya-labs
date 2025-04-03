using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.Template;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IFormTemplate : IFnDocument
    {
        [Get("/document/template")]
        Task<ApiErrorResult<IEnumerable<GetTemplateResult>>> GetTemplates(GetTemplateRequest param);

        [Get("/document/template/{id}")]
        Task<ApiErrorResult<GetTemplateDetailResult>> GetTemplateDetail(string id);

        [Post("/document/template")]
        Task<ApiErrorResult> AddTemplate([Body] AddTemplateRequest body);

        [Put("/document/template")]
        Task<ApiErrorResult> UpdateTemplate([Body] UpdateTemplateRequest body);
    }
}
