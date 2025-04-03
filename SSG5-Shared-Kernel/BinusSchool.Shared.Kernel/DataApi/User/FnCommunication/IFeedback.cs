using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Feedback;
using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BinusSchool.Data.Api.User.FnCommunication
{
    public interface IFeedback : IFnCommunication
    {
        [Get("/feedback/types")]
        Task<ApiErrorResult<IEnumerable<GetFeedbackTypeResult>>> GetFeedbackTypes(GetFeedbackTypeRequest request);

        [Post("/feedback")]
        Task<ApiErrorResult> AddFeedback([Body] AddFeedbackRequest body);
    }
}
