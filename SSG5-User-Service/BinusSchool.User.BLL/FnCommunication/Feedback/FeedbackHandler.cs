using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.User.FnCommunication.Feedback;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.User.FnCommunication.Feedback.Validator;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using BinusSchool.Persistence.UserDb.Entities.Student;
using BinusSchool.Persistence.UserDb.Entities.School;
using NPOI.OpenXmlFormats.Spreadsheet;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.User.FnCommunication;

namespace BinusSchool.User.FnCommunication.Feedback
{
    public class FeedbackHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IMessage _apiMessage;

        public FeedbackHandler(IUserDbContext userDbContext, IMessage apiMessage, IMachineDateTime dateTime)
        {
            _dbContext = userDbContext;
            _dateTime = dateTime;
            _apiMessage = apiMessage;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddFeedbackRequest, AddFeedbackValidator>();

            var currentAcademicYear = await _dbContext.Entity<MsPeriod>()
              .Include(x => x.Grade)
                  .ThenInclude(x => x.MsLevel)
                      .ThenInclude(x => x.MsAcademicYear)
              .Where(x => x.Grade.MsLevel.MsAcademicYear.IdSchool == body.IdSchool)
              .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
              .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
              .Select(x => x.Grade.MsLevel.IdAcademicYear).FirstOrDefaultAsync();
            if (currentAcademicYear == null)
                throw new Exception("The master data period is not available. Please contact your system administrator");

            var feedbackType = await _dbContext.Entity<MsFeedbackType>()
                .Include(x => x.UserCc)
                .Include(x => x.UserTo)
                .Where(x => x.Id == body.FeedbackType || x.Code == body.FeedbackType)
                .FirstOrDefaultAsync();
            if (feedbackType == null)
                throw new NotFoundException("Feedback type not found");

            var message = _dbContext.Entity<TrMessage>()
            .Include(x => x.MessageAttachments)
            .Include(X => X.MessageRecepients)
            .FirstOrDefault(x => x.Id == body.IdMessage);
            var trMessage = message ?? new TrMessage();

            trMessage.IdSender = AuthInfo.UserId;
            trMessage.IdFeedbackType = feedbackType.Id;
            trMessage.Content = HtmlEncoder.Default.Encode(body.Content);
            trMessage.Subject = body.Subject;
            trMessage.IsAllowReply = true;
            trMessage.IsSetSenderAsSchool = false;
            trMessage.IsDraft = body.IsDraft;
            trMessage.IsMarkAsPinned = false;
            trMessage.Type = UserMessageType.Feedback;
            trMessage.Status = StatusMessage.Approved;

            var recepients = new List<string>();
            if (feedbackType.UserTo != null) recepients.Add(feedbackType.UserTo.Id);
            if (feedbackType.UserCc != null) recepients.Add(feedbackType.UserCc.Id);

            var attachments = body.Attachments;

            if (message != null)
            {
                _dbContext.Entity<TrMessage>().Update(trMessage);

                var existingRecepients = await _dbContext.Entity<TrMessageRecepient>().Where(x => x.IdMessage == trMessage.Id).Select(x => x.IdRecepient).ToListAsync(CancellationToken);

                var deleteRecepients = existingRecepients.Where(x => !recepients.Contains(x)).ToList();
                if (deleteRecepients.Count > 0)
                {
                    foreach (var r in deleteRecepients)
                    {
                        var recepient = trMessage.MessageRecepients.Where(x => x.IdRecepient == r).FirstOrDefault();
                        if (recepient == null)
                            throw new NotFoundException("Recepient not found");
                        recepient.IsActive = false;
                    }
                }

                var deleteAttachments = trMessage.MessageAttachments.Where(x => x.IdMessage == trMessage.Id).FirstOrDefault();
                if (deleteAttachments != null)
                {
                    deleteAttachments.IsActive = false;
                    _dbContext.Entity<TrMessageAttachment>().Update(deleteAttachments);
                }
                else
                {
                }

                recepients = recepients.Where(x => !existingRecepients.Contains(x)).ToList();
            }
            else
            {
                trMessage.Id = Guid.NewGuid().ToString();
                _dbContext.Entity<TrMessage>().Add(trMessage);
            }

            if (!trMessage.IsDraft)
            {
                if (!string.IsNullOrEmpty(body.IdMessage))
                {
                    _dbContext.Entity<TrMessageRecepient>().Add(new TrMessageRecepient
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdRecepient = trMessage.IdSender,
                        IdMessage = trMessage.Id,
                        MessageFolder = MessageFolder.Sent,
                        IsRead = true
                    });
                }
                else if (!string.IsNullOrEmpty(body.IdMessage))
                {
                    var sender = trMessage.MessageRecepients.Where(x => x.IdRecepient == trMessage.IdSender).FirstOrDefault();
                    if (sender == null)
                        throw new NotFoundException("Sender not found");

                    sender.MessageFolder = MessageFolder.Sent;
                    _dbContext.Entity<TrMessageRecepient>().Update(sender);
                }
            }

            _dbContext.Entity<TrMessageRecepient>().Add(new TrMessageRecepient
            {
                Id = Guid.NewGuid().ToString(),
                IdRecepient = AuthInfo.UserId,
                IdMessage = trMessage.Id,
                MessageFolder = MessageFolder.Sent,
                IsRead = true
            });

            await _dbContext.Entity<TrMessageRecepient>().AddRangeAsync(recepients.Select(x => new TrMessageRecepient
            {
                Id = Guid.NewGuid().ToString(),
                IdRecepient = x,
                IdMessage = trMessage.Id,
                MessageFolder = MessageFolder.Inbox,
                IsRead = false
            }), CancellationToken);

            if (body.Attachments != null)
            {
                var dataAttachments = body.Attachments.Select(x => x.Url).FirstOrDefault();

                if (!string.IsNullOrEmpty(dataAttachments))
                {
                    await _dbContext.Entity<TrMessageAttachment>().AddRangeAsync(body.Attachments.Select(x => new TrMessageAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdMessage = trMessage.Id,
                        Url = x.Url,
                        Filename = x.Filename,
                        Filetype = x.Filetype,
                        Filesize = x.Filesize
                    }), CancellationToken);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _apiMessage.QueueNotificationMessage(new QueueMessagesRequest
            {
                IdSchool = body.IdSchool,
            });

            //send notif to teacher
            await SendNotifToTeacher(recepients, AuthInfo.UserId, trMessage);

            return Request.CreateApiResult2();
        }

        public async Task SendNotifToTeacher(List<string> recepients, string userId, TrMessage trMessage)
        {
            IDictionary<string, object> paramTemplateEmailNotification = new Dictionary<string, object>();

            var student = await _dbContext.Entity<MsUser>()
                       .Include(x => x.UserSchools).ThenInclude(x => x.School)
                       .Where(x => x.Id == userId)
                       .Select(x => new
                       {
                           senderName = x.DisplayName,
                           idSchool = x.UserSchools.Select(y => y.IdSchool).FirstOrDefault()
                       }).FirstOrDefaultAsync(CancellationToken);

            paramTemplateEmailNotification.Add("id", trMessage.Id);
            paramTemplateEmailNotification.Add("messageType", trMessage.Type);
            paramTemplateEmailNotification.Add("sendBy", student.senderName);

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "FD1")
                {
                    IdSchool = student.idSchool,
                    IdRecipients = recepients,
                    KeyValues = paramTemplateEmailNotification
                });
                collector.Add(message);
            }
        }
    }
}
