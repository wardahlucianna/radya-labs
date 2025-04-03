using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnCommunication.Message.Validator;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using BinusSchool.Persistence.UserDb.Entities.School;
using System.Text.Encodings.Web;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.User.FnCommunication.Message
{
    public class SetMessageEndConversationHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;

        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;

        public IDictionary<string, object> paramTemplateEmailNotification = new Dictionary<string, object>();

        public SetMessageEndConversationHandler
        (
            IUserDbContext userDbContext,
            IConfiguration configuration,
            IMachineDateTime dateTime
        )
        {
            _dbContext = userDbContext;
            _configuration = configuration;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var body = await Request.ValidateBody<SetMessageEndConversationRequest, SetMessageEndConversationValidator>();

            var user = _dbContext.Entity<MsUser>()
                .Include(x => x.UserRoles)
                .FirstOrDefault(x => x.Id == body.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            var role = user.UserRoles.Where(x => x.IdUser == body.UserId).FirstOrDefault();
            if (role == null)
                throw new NotFoundException("User Role not found");

            var IdMessage = Guid.NewGuid().ToString();

            var messageBefore = _dbContext.Entity<TrMessage>().FirstOrDefault(x => x.Id == body.MessageId);
            if (messageBefore == null)
                throw new NotFoundException("previous message is not found");

            var messagePrent = _dbContext.Entity<TrMessage>().FirstOrDefault(x => x.Id == messageBefore.ParentMessageId);
            if (messagePrent == null)
            {
                messagePrent = _dbContext.Entity<TrMessage>().FirstOrDefault(x => x.Id == body.MessageId);
            }

            if (!messagePrent.IsAllowReply)
                throw new BadRequestException("This message cannot be replied");
            else
            {
                if (messagePrent.ReplyStartDate.HasValue && _dateTime.ServerTime.Date < messagePrent.ReplyStartDate.Value.Date)
                    throw new BadRequestException("This message replied period hasn't started");
                if (messagePrent.ReplyStartDate.HasValue && _dateTime.ServerTime.Date > messagePrent.ReplyEndDate.Value.Date)
                    throw new BadRequestException("This message replied period is over, it no longer can be replied");
            }

            _dbContext.Entity<TrMessage>().Add(new TrMessage
            {
                Id = IdMessage,
                IdSender = body.UserId,
                Type = messagePrent.Type,
                Subject = "Reply To: " + messagePrent.Subject,
                IdMessageCategory = messagePrent.IdMessageCategory,
                Content = HtmlEncoder.Default.Encode(body.Reason),
                ParentMessageId = messagePrent.Id,
                IdFeedbackType = messagePrent.IdFeedbackType,
                IsAllowReply = true,
                ReplyStartDate = messagePrent.ReplyStartDate,
                ReplyEndDate = messagePrent.ReplyEndDate,
                EndConversation = true,
                ReasonEndConversation = body.Reason
            });

            _dbContext.Entity<TrMessageRecepient>().Add(new TrMessageRecepient
            {
                Id = Guid.NewGuid().ToString(),
                IdRecepient = body.UserId,
                IdMessage = IdMessage,
                MessageFolder = MessageFolder.Sent,
                IsRead = true,
                IsActive = true
            });

            _dbContext.Entity<TrMessageRecepient>().Add(new TrMessageRecepient
            {
                Id = Guid.NewGuid().ToString(),
                IdRecepient = messageBefore.IdSender,
                IdMessage = IdMessage,
                MessageFolder = MessageFolder.Inbox,
                IsRead = false,
                IsActive = true
            });

            if (messageBefore == null)
                throw new NotFoundException("Message not found");

            messageBefore.EndConversation = true;
            messageBefore.ReasonEndConversation = body.Reason;

            _dbContext.Entity<TrMessage>().Update(messageBefore);
                
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
