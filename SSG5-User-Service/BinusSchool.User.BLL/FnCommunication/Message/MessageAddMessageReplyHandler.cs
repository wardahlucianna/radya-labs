using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
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
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cmp;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageAddMessageReplyHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IMessage _apiMessage;

        public MessageAddMessageReplyHandler(
            IUserDbContext dbContext,
            IMachineDateTime dateTime,
            IMessage apiMessage
            )
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _apiMessage = apiMessage;

        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddMessageReplyRequest, AddMessageReplyValidator>();

            var IdMessage = Guid.NewGuid().ToString();

            var messageBefore = _dbContext.Entity<TrMessage>().FirstOrDefault(x => x.Id == body.MessageId);
            if (messageBefore == null)
                throw new NotFoundException("previous message is not found");

            var messagePrent = _dbContext.Entity<TrMessage>().FirstOrDefault(x => x.Id == body.ParentMessageId);
            if (messagePrent == null)
                throw new NotFoundException("parent message is not found");

            if (!messagePrent.IsAllowReply)
                throw new BadRequestException("This message cannot be replied");
            else
            {
                if (messagePrent.ReplyStartDate.HasValue && _dateTime.ServerTime.Date < messagePrent.ReplyStartDate.Value.Date)
                    throw new BadRequestException("This message replied period hasn't started");
                if (messagePrent.ReplyStartDate.HasValue && _dateTime.ServerTime.Date > messagePrent.ReplyEndDate.Value.Date)
                    throw new BadRequestException("This message replied period is over, it no longer can be replied");
            }

            if(body.IsEdit)
            {
                messageBefore.Content = HtmlEncoder.Default.Encode(body.Content);
                _dbContext.Entity<TrMessage>().Update(messageBefore);
                IdMessage = messageBefore.Id;
                var attachments = body.Attachments;
                var existingAttachments = await _dbContext.Entity<TrMessageAttachment>().Where(x => x.IdMessage == messageBefore.Id).ToListAsync(CancellationToken);
                var deleteAttachments = existingAttachments.Where(x => !body.Attachments.Any(y => y.Url == x.Url)).ToList();
                 
                if (deleteAttachments.Count > 0)
                {
                    foreach (var a in deleteAttachments)
                    {
                        var attachment = messageBefore.MessageAttachments.Where(x => x.Id == a.Id).FirstOrDefault();
                        if (attachment == null)
                            throw new NotFoundException("Attachment not found");
                        attachment.IsActive = false;

                        _dbContext.Entity<TrMessageAttachment>().Update(attachment);
                    }
                }
                attachments = body.Attachments.Where(x => !existingAttachments.Any(y => y.Url == x.Url)).ToList();

                _dbContext.Entity<TrMessageAttachment>().AddRange(attachments.Select(x => new TrMessageAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdMessage = IdMessage,
                    Url = x.Url,
                    Filename = x.Filename,
                    Filetype = x.Filetype,
                    Filesize = x.Filesize,
                    IsActive = true
                }));
            } 
            else
            {
                _dbContext.Entity<TrMessage>().Add(new TrMessage
                {
                    Id = IdMessage,
                    IdSender = body.IdSender,
                    Type = messagePrent.Type,
                    Subject = "Reply To: " + messagePrent.Subject,
                    IdMessageCategory = messagePrent.IdMessageCategory,
                    Content = HtmlEncoder.Default.Encode(body.Content),
                    ParentMessageId = body.ParentMessageId,
                    IdFeedbackType = messagePrent.IdFeedbackType,
                    IsAllowReply = true,
                    ReplyStartDate = messagePrent.ReplyStartDate,
                    ReplyEndDate = messagePrent.ReplyEndDate,
                    IsSetSenderAsSchool = messagePrent.IsSetSenderAsSchool
                });

                _dbContext.Entity<TrMessageAttachment>().AddRange(body.Attachments.Select(x => new TrMessageAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdMessage = IdMessage,
                    Url = x.Url,
                    Filename = x.Filename,
                    Filetype = x.Filetype,
                    Filesize = x.Filesize,
                    IsActive = true
                }));

                _dbContext.Entity<TrMessageRecepient>().Add(new TrMessageRecepient
                {
                    Id = Guid.NewGuid().ToString(),
                    IdRecepient = body.IdSender,
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
            }

            

            

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _apiMessage.QueueNotificationMessage(new QueueMessagesRequest
            {
                IdSchool = body.IdSchool,
            });

            await SendReplyMessage(messagePrent, body, messageBefore, IdMessage);

            return Request.CreateApiResult2();
        }

        public async Task SendReplyMessage(TrMessage messageParent, AddMessageReplyRequest body, TrMessage messageBefore, string IdMessageNow)
        {
            IDictionary<string, object> paramTemplateEmailNotification = new Dictionary<string, object>();

            var dataSender = string.Empty;
            
            dataSender = body.IdSender != messageParent.IdSender
                ? await _dbContext.Entity<MsUser>().Where(x => x.Id == body.IdSender).Select(x => x.DisplayName).FirstOrDefaultAsync(CancellationToken)
                : !messageParent.IsSetSenderAsSchool
                    ? await _dbContext.Entity<MsUser>().Where(x => x.Id == messageParent.IdSender).Select(x => x.DisplayName).FirstOrDefaultAsync(CancellationToken)
                    : messageParent.IdSender == Guid.Empty.ToString()
                        ? "System"
                        : await _dbContext.Entity<MsUserSchool>()
                                                    .Include(x => x.School)
                                                    .Where(x => x.IdUser == messageParent.IdSender)
                                                    .Select(x => x.School.Description).FirstOrDefaultAsync(CancellationToken);

            var school = await _dbContext.Entity<MsUser>()
                                .Include(x => x.UserSchools)
                                    .ThenInclude(x => x.School)
                                .Where(f => f.Id == body.IdSender)
                                .Select(f => new
                                {
                                    idSchool = f.UserSchools.Select(x => x.IdSchool).FirstOrDefault(),
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            paramTemplateEmailNotification.Add("id", IdMessageNow);
            paramTemplateEmailNotification.Add("sendBy", dataSender);
            paramTemplateEmailNotification.Add("titleMessage", $"Reply to : {messageParent.Subject}");
            paramTemplateEmailNotification.Add("dateTimes", _dateTime.ServerTime.ToString("dd MMMM yyyy hh:mm:ss"));
            paramTemplateEmailNotification.Add("messageType", messageParent.Type.ToString());

            if (messageParent.Type == UserMessageType.Private)
            {
                if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS8")
                    {
                        IdSchool = school.idSchool,
                        IdRecipients = new[] { messageBefore.IdSender },
                        KeyValues = paramTemplateEmailNotification
                    });

                    collector.Add(message);
                }
            }

            if (messageParent.Type == UserMessageType.Feedback)
            {
                if (KeyValues.TryGetValue("collector1", out var collect1) && collect1 is ICollector<string> collector1)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "FD2")
                    {
                        IdSchool = school.idSchool,
                        IdRecipients = new[] { messageBefore.IdSender },
                        KeyValues = paramTemplateEmailNotification
                    });

                    collector1.Add(message);
                }
            }
        }
    }
}
