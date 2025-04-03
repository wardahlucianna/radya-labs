using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.AnswerSet;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IAnswerSet : IFnSchool
    {
        [Get("/answer-set")]
        Task<ApiErrorResult<IEnumerable<AnswerSetResult>>> GetAnswerSets(GetAnswerSetRequest request);

        [Get("/answer-set/detail/{id}")]
        Task<ApiErrorResult<AnswerSetDetailResult>> GetAnswerSetDetail(string id);

        [Post("/answer-set")]
        Task<ApiErrorResult> AddAnswerSet([Body] AddAnswerSetRequest body);

        [Put("/answer-set")]
        Task<ApiErrorResult> UpdateAnswerSet([Body] UpdateAnswerSetRequest body);

        [Delete("/answer-set")]
        Task<ApiErrorResult> DeleteAnswerSet([Body] IEnumerable<string> ids);

        [Get("/answer-set/list-answer-set")]
        Task<ApiErrorResult<IEnumerable<GetCopyListAnswerSetHandlerResult>>> GetCopyListAnswerSets(GetCopyListAnswerSetRequest request);

        [Post("/answer-set/list-answer-set")]
        Task<ApiErrorResult> CopyListAnswerSet([Body] CopyListAnswerSetRequest body);
    }
}
