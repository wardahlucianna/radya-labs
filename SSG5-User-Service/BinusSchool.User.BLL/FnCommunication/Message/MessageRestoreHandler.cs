using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnCommunication.Message.Validator;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageRestoreHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        public MessageRestoreHandler(IUserDbContext userDbContext)
        {
            _dbContext = userDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<MessageRestoreRequest, MessageRestoreValidator>();

            var message = _dbContext.Entity<TrMessage>()
                .Include(x => x.MessageRecepients)
                .FirstOrDefault(x => x.Id == body.MessageId);
            if (message == null)
                throw new NotFoundException("Message not found");

            var recepient = message.MessageRecepients.Where(x => x.IdRecepient == body.UserId).FirstOrDefault();
                if (recepient == null)
                    throw new NotFoundException("Recepient not found");
                

            if (message.IdSender == body.UserId && !message.IsDraft)
            {
                recepient.MessageFolder = MessageFolder.Sent;
            }
            else if (message.IdSender == body.UserId && message.IsDraft)
            {
                recepient.IsActive = false;
            }
            else if (message.IdSender != body.UserId)
            {
                recepient.MessageFolder = MessageFolder.Inbox;
            }

            _dbContext.Entity<TrMessageRecepient>().Update(recepient);
            await _dbContext.SaveChangesAsync(CancellationToken); 

            return Request.CreateApiResult2();
        }    
    }
}