using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnCommunication.Message.Validator;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Cmp;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageUnsendMessageHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMessage _apiMessage;

        public MessageUnsendMessageHandler(IUserDbContext dbContext, IMessage apiMessage)
        {
            _dbContext = dbContext;
            _apiMessage = apiMessage;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UnsendMessageRequest, UnsendMessageValidator>();

            var message = _dbContext.Entity<TrMessage>()
                .Include(x => x.MessageAttachments)
                .Include(X => X.MessageRecepients)
                .FirstOrDefault(x => x.Id == body.IdMessage);
            if (message == null) throw new NotFoundException("Message not found");
            if (message.IdSender != body.IdUser) throw new NotFoundException("The user not allowed to unsend this message");

            message.IsUnsend = true;
            message.StatusUnsend = StatusMessage.Approved;

            if (message.Type != UserMessageType.AscTimetable && message.Type != UserMessageType.GenerateSchedule && message.ParentMessageId == null)
            {
                var messageCategory = _dbContext.Entity<MsMessageCategory>()
                    .Include(x => x.MessageApproval)
                        .ThenInclude(x => x.ApprovalStates)
                    .OrderBy(x => x.MessageApproval.ApprovalStates.FirstOrDefault().Number).FirstOrDefault(x => x.Id == message.IdMessageCategory);
                if (messageCategory != null)
                {
                    if (messageCategory.MessageApproval.ApprovalStates.Count > 0)
                    {
                        message.StatusUnsend = StatusMessage.OnProgress;
                    }
                }
            }
            _dbContext.Entity<TrMessage>().Update(message);

            _dbContext.Entity<TrMessageRecepient>().Add(new TrMessageRecepient
            {
                Id = Guid.NewGuid().ToString(),
                IdRecepient = message.IdSender,
                IdMessage = message.Id,
                MessageFolder = MessageFolder.Unsend,
                IsRead = true
            });

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _apiMessage.QueueNotificationMessage(new QueueMessagesRequest
            {
                IdSchool = body.IdSchool,
            });
            return Request.CreateApiResult2();
        }
    }
}
