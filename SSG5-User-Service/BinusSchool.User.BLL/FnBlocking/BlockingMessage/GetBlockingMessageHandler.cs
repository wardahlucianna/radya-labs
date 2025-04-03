using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.BlockingMessage;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnBlocking.BlockingMessage
{
    public class GetBlockingMessageHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public GetBlockingMessageHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetBlockingMessageRequest>();


            var query = await _dbContext.Entity<MsBlockingMessage>()
                        .Where(x=> x.IdSchool == param.IdSchool)
                        .Select(x => new GetBlockingMessageResult
                        {
                            Id = x.Id,
                            BlockingMessage = x.Content
                        }).FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }
    }
}
