using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.BlockingMessage;
using Refit;

namespace BinusSchool.Data.Api.User.FnBlocking.BlockingMessage
{
    public interface IBlockingMessage : IFnBlocking
    {
        [Get("/user-blocking/blocking-message")]
        Task<ApiErrorResult<GetBlockingMessageResult>> GetBlockingMessage(GetBlockingMessageRequest query);

        [Get("/user-blocking/blocking-message/{id}")]
        Task<ApiErrorResult<GetBlockingMessageResult>> GetBlockingMessageDetail(string id);

        [Post("/user-blocking/blocking-message")]
        Task<ApiErrorResult> UpdateBlockingMessage([Body] UpdateBlockingMessageRequest body);
    }
}
