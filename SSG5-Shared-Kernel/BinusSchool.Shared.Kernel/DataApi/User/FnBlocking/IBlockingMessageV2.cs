using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.BlockingMessageV2;
using Refit;

namespace BinusSchool.Data.Api.User.FnBlocking
{
    public interface IBlockingMessageV2 : IFnBlocking
    {
        [Get("/user-blocking/blocking-message-v2")]
        Task<ApiErrorResult<IEnumerable<GetBlockingMessageResultV2>>> GetBlockingMessageV2(GetBlockingMessageRequestV2 query);

        [Get("/user-blocking/blocking-message-v2/detail/{id}")]
        Task<ApiErrorResult<GetBlockingMessageResultV2>> GetBlockingMessageDetailV2(string id);

        [Post("/user-blocking/blocking-message-v2")]
        Task<ApiErrorResult> AddBlockingMessageV2([Body] AddBlockingMessageRequestV2 body);

        [Put("/user-blocking/blocking-message-v2")]
        Task<ApiErrorResult> UpdateBlockingMessageV2([Body] UpdateBlockingMessageRequestV2 body);

        [Delete("/user-blocking/blocking-message-v2")]
        Task<ApiErrorResult> DeleteBlockingMessageV2([Body] IEnumerable<string> ids);
    }
}
