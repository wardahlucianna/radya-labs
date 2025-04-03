using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnCommunication.Message.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageDeleteMessageHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public MessageDeleteMessageHandler(IUserDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteMessageRequest, DeleteMessageValidator>();

            if (body.IdMessage.Any())
            {
                var deleteds = await _dbContext.Entity<TrMessageRecepient>()
                                                 .Where(x => body.IdMessage.Contains(x.IdMessage) && x.IdRecepient == body.IdUser && x.MessageFolder==body.MessageFolder)
                                                 .ToListAsync(CancellationToken);

                if (body.IsDeletePermanent == true)
                {
                    foreach (var deleted in deleteds)
                    {
                        deleted.IsActive = false;
                        _dbContext.Entity<TrMessageRecepient>().Update(deleted);
                    }
                }
                else
                {
                    foreach (var deleted in deleteds)
                    {
                        deleted.MessageFolder = MessageFolder.Trash;
                        _dbContext.Entity<TrMessageRecepient>().Update(deleted);
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
            }

            return Request.CreateApiResult2();
        }
    }
}
