using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.Sourcedata;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface ISourcedata : IFnDocument
    {
        [Get("/document/sourcedata")]
        Task<ApiErrorResult<IEnumerable<GetSourcedataResult>>> GetSourcedatas(CollectionRequest param);

        [Get("/document/sourcedata/{id}")]
        Task<ApiErrorResult<GetSourcedataDetailResult>> GetSourcedataDetail(string id);

        [Post("/document/sourcedata")]
        Task<ApiErrorResult> AddSourcedata([Body] AddSourcedataRequest body);

        [Put("/document/sourcedata")]
        Task<ApiErrorResult> UpdateSourcedata([Body] UpdateSourcedataRequest body);
    }
}
