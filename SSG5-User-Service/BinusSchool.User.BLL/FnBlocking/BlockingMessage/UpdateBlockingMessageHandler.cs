using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.BlockingMessage;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnBlocking.BlockingMessage.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.User.FnBlocking.BlockingMessage
{
    public class UpdateBlockingMessageHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly IUserDbContext _dbContext;

        public UpdateBlockingMessageHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateBlockingMessageRequest, UpdateBlockingMessageValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var existBlockingMessage = await _dbContext.Entity<MsBlockingMessage>()
                .FirstOrDefaultAsync(x => x.Id == body.Id && x.IdSchool == body.IdSchool, CancellationToken);
            if (existBlockingMessage is null)
            {
                var idBlockingMessage = Guid.NewGuid().ToString();

                var newBlockingMessage = new MsBlockingMessage
                {
                    Id = idBlockingMessage,
                    IdSchool = body.IdSchool,
                    Content = body.BlockingMessage,
                };

                _dbContext.Entity<MsBlockingMessage>().Add(newBlockingMessage);
            }
            else
            {
                if (existBlockingMessage is null)
                {
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Blocking Message"], "Id", body.Id));
                }
                existBlockingMessage.Content = body.BlockingMessage;

                _dbContext.Entity<MsBlockingMessage>().Update(existBlockingMessage);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();

        }
    }
}
