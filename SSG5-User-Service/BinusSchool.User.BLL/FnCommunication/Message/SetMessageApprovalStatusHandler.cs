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
using BinusSchool.Data.Api.User.FnCommunication;

namespace BinusSchool.User.FnCommunication.Message
{
    public class SetMessageApprovalStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMessage _apiMessage;

        public IDictionary<string, object> paramTemplateEmailNotification = new Dictionary<string, object>();

        public SetMessageApprovalStatusHandler(IUserDbContext userDbContext, IConfiguration configuration, IMessage apiMessage)
        {
            _dbContext = userDbContext;
            _configuration = configuration;
            _apiMessage = apiMessage;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetMessageApprovalStatusRequest, SetMessageApprovalStatusValidator>();

            var user = _dbContext.Entity<MsUser>()
                .Include(x => x.UserRoles)
                .FirstOrDefault(x => x.Id == body.UserId);
            if (user == null)
                throw new NotFoundException("User not found");

            var role = user.UserRoles.Where(x => x.IdUser == body.UserId).FirstOrDefault();
            if (role == null)
                throw new NotFoundException("Role not found");

            var message = _dbContext.Entity<TrMessage>()
                .Include(x => x.MessageCategory).ThenInclude(x => x.MessageApproval).ThenInclude(x => x.ApprovalStates)
                .Include(x => x.MessageApprovals)
                .Include(x => x.MessageRecepients)
                .Where(x => x.Id == body.MessageId)
                .FirstOrDefault();
            if (message == null)
                throw new NotFoundException("Message not found");

            var messageCategory = _dbContext.Entity<MsMessageCategory>()
                .Include(x => x.MessageApproval)
                    .ThenInclude(x => x.ApprovalStates)
                .FirstOrDefault(x => x.Id == message.IdMessageCategory);
            if (messageCategory == null)
                throw new NotFoundException("Message category not found");

            var dataApproval = await (from a in _dbContext.Entity<TrMessageApproval>()
                                      where a.IdMessage == body.MessageId
                                      orderby a.DateIn descending
                                      select new TrMessageApproval
                                      {
                                          IdMessage = a.IdMessage,
                                          StateNumber = a.StateNumber,
                                          IsApproved = a.IsApproved,
                                          IdRole = a.IdRole,
                                          IdUser = a.IdUser,
                                          Reason = a.Reason,
                                          DateUp = a.DateUp,
                                      }).FirstOrDefaultAsync(CancellationToken);

            var dataApprovalUnsend = await (from a in _dbContext.Entity<TrMessageApproval>()
                                            where a.IdMessage == body.MessageId
                                            orderby a.DateIn ascending
                                            select new TrMessageApproval
                                            {
                                                IdMessage = a.IdMessage,
                                                StateNumber = a.StateNumber,
                                                IsApproved = a.IsApproved,
                                                IdRole = a.IdRole,
                                                IdUser = a.IdUser,
                                                Reason = a.Reason,
                                                DateUp = a.DateUp,
                                            }).FirstOrDefaultAsync(CancellationToken);

            if (message.IsUnsend)
            {
                if (dataApprovalUnsend.StateNumber == 1 && dataApprovalUnsend.IsUnsendApproved == false && dataApprovalUnsend.IsApproved == false)
                {
                    if (message.MessageCategory.MessageApproval.ApprovalStates.Where(x => x.MessageApproval.ApprovalStates.Any(y => y.Number == 1)).First().IdUser != body.UserId)
                    {
                        throw new NotFoundException("The user not allowed to approved/reject this unsend message approval state 1");
                    }
                }

                if (dataApprovalUnsend.StateNumber == 2 && dataApprovalUnsend.IsUnsendApproved == false && dataApprovalUnsend.IsApproved == false)
                {
                    if (message.MessageCategory.MessageApproval.ApprovalStates.Where(x => x.MessageApproval.ApprovalStates.Any(y => y.Number == 2)).Last().IdUser != body.UserId)
                    {
                        throw new NotFoundException("The user not allowed to approved/reject this unsend message approval state 2");
                    }
                }
            }
            else
            {
                if (dataApproval.StateNumber == 1 && dataApproval.IsApproved == false && dataApproval.DateUp == null)
                {
                    if (message.MessageCategory.MessageApproval.ApprovalStates.Where(x => x.MessageApproval.ApprovalStates.Any(y => y.Number == 1)).First().IdUser != body.UserId)
                    {
                        throw new NotFoundException("The user not allowed to approved/reject this message approval state 1");
                    }
                }

                if (dataApproval.StateNumber == 2 && dataApproval.IsApproved == false && dataApproval.DateUp == null)
                {
                    if (message.MessageCategory.MessageApproval.ApprovalStates.Where(x => x.MessageApproval.ApprovalStates.Any(y => y.Number == 2)).Last().IdUser != body.UserId)
                    {
                        throw new NotFoundException("The user not allowed to approved/reject this message approval state 2");
                    }
                }
            }

            var messageToBeChanged = message.MessageApprovals
                .Where(x => x.IdUser == body.UserId)
                .OrderByDescending(x => x.DateIn)
                .FirstOrDefault();

            if (messageToBeChanged != null)
            {
                messageToBeChanged.IdUser = user.Id;

                if (message.IsUnsend)
                {
                    messageToBeChanged.UnsendReason = body.Reason;
                    messageToBeChanged.IsUnsendApproved = body.IsApproved;
                }
                else
                {
                    messageToBeChanged.Reason = body.Reason;
                    messageToBeChanged.IsApproved = body.IsApproved;
                }

                _dbContext.Entity<TrMessageApproval>().Update(messageToBeChanged);
            }
            else
                throw new NotFoundException("The user not allowed to approved/reject this message or the message already approved/rejected");

            var anotherStep = messageCategory.MessageApproval.ApprovalStates
                .Where(x => x.Number > messageToBeChanged.StateNumber)
                .FirstOrDefault();

            if (message.IsUnsend)
            {
                if (anotherStep != null)
                {
                    if (body.IsApproved == false)
                    {
                        message.StatusUnsend = StatusMessage.UnsendRejected;
                    }
                }
                else
                {
                    message.StatusUnsend = body.IsApproved ? StatusMessage.Approved : StatusMessage.UnsendRejected;
                }

                //message.Status = body.IsApproved ? StatusMessage.Approved : StatusMessage.UnsendRejected;

                _dbContext.Entity<TrMessage>().Update(message);

                await _dbContext.SaveChangesAsync(CancellationToken);
            }
            else
            {
                if (anotherStep != null)
                {
                    if (body.IsApproved == true)
                    {
                        _dbContext.Entity<TrMessageApproval>().Add(new TrMessageApproval
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMessage = message.Id,
                            StateNumber = anotherStep.Number,
                            IdRole = anotherStep.IdRole,
                            IdUser = anotherStep.IdUser
                        });

                        //send email from approver 1 to approver 2 for approver message
                        await SendEmailToApproval2(message);
                    }
                    else
                    {
                        message.Status = StatusMessage.Rejected;

                        if (message.IsEdit)
                        {
                            //remove new message
                            var newMessage = await _dbContext.Entity<TrMessageRecepient>().Where(x => x.IdMessage == message.Id).ToListAsync(CancellationToken);
                            if (newMessage.Count > 0)
                            {
                                foreach (var item in newMessage)
                                {
                                    var recipient = message.MessageRecepients.Where(x => x.Id == item.Id).FirstOrDefault();
                                    if (recipient == null)
                                        throw new NotFoundException("Message not found");
                                    recipient.IsActive = false;
                                    _dbContext.Entity<TrMessageRecepient>().Update(recipient);
                                }
                            }

                            // Activate last approved message
                            var previousApprovedMessage = await _dbContext.Entity<TrMessage>().Where(x => x.Id == message.ParentMessageId).IgnoreQueryFilters().OrderByDescending(x => x.DateIn).FirstOrDefaultAsync();
                            if (previousApprovedMessage == null)
                                throw new NotFoundException("Previous approved message not found");
                            previousApprovedMessage.IsActive = true;
                            previousApprovedMessage.Status = StatusMessage.EditRejected;

                            _dbContext.Entity<TrMessage>().Update(previousApprovedMessage);
                        }

                        _dbContext.Entity<TrMessage>().Update(message);

                        //send email to sender reject message
                        await SendEmailApproveRejectToSender(message);
                    }
                }
                else
                {
                    message.Status = body.IsApproved ? StatusMessage.Approved : StatusMessage.Rejected;

                    if (message.IsEdit)
                    {
                        if (body.IsApproved)
                        {
                            //remove old message
                            var oldMessage = await _dbContext.Entity<TrMessageRecepient>().Where(x => x.IdMessage == message.ParentMessageId).ToListAsync(CancellationToken);
                            if (oldMessage.Count > 0)
                            {
                                foreach (var item in oldMessage)
                                {
                                    item.IsActive = false;
                                    _dbContext.Entity<TrMessageRecepient>().Update(item);
                                }
                            }
                        }
                        else
                        {
                            //remove new message
                            var newMessage = await _dbContext.Entity<TrMessageRecepient>().Where(x => x.IdMessage == message.Id).ToListAsync(CancellationToken);
                            if (newMessage.Count > 0)
                            {
                                foreach (var item in newMessage)
                                {
                                    var recipient = message.MessageRecepients.Where(x => x.Id == item.Id).FirstOrDefault();
                                    if (recipient == null)
                                        throw new NotFoundException("Message not found");
                                    recipient.IsActive = false;
                                    _dbContext.Entity<TrMessageRecepient>().Update(recipient);
                                }
                            }

                            // Activate last approved message
                            var previousApprovedMessage = await _dbContext.Entity<TrMessage>().Where(x => x.Id == message.ParentMessageId).IgnoreQueryFilters().OrderByDescending(x => x.DateIn).FirstOrDefaultAsync();
                            if (previousApprovedMessage == null)
                                throw new NotFoundException("Previous approved message not found");
                            previousApprovedMessage.IsActive = true;
                            previousApprovedMessage.Status = StatusMessage.EditRejected;

                            _dbContext.Entity<TrMessage>().Update(previousApprovedMessage);
                        }
                    }

                    _dbContext.Entity<TrMessage>().Update(message);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);

                await _apiMessage.QueueNotificationMessage(new QueueMessagesRequest
                {
                    IdSchool = body.IdSchool,
                });

                if (message.Status == StatusMessage.Approved)
                {
                    await SendEmailApproveRejectToSender(message);

                    if (message.IsSendEmail)
                    {
                        await SendEmailToRecipient(message);
                    }
                    else
                    {
                        await SendNotifToStudent(message, body.UserId);
                    }
                }
            }

            await _apiMessage.QueueNotificationMessage(new QueueMessagesRequest
            {
                IdSchool = body.IdSchool,
            });

            return Request.CreateApiResult2();
        }

        //send email to recipient
        public async Task SendEmailToRecipient(TrMessage trMessage)
        {
            var idRecipient = await _dbContext.Entity<TrMessageRecepient>()
                                .Where(x => x.IdMessage == trMessage.Id && x.MessageFolder == MessageFolder.Inbox)
                                .Select(x => new
                                {
                                    Id = x.IdRecepient
                                }).ToListAsync(CancellationToken);

            var listIdRecipient = new List<string>();

            foreach (var item in idRecipient)
            {
                listIdRecipient.Add(item.Id);
            }

            var school = _dbContext.Entity<MsUser>()
                        .Include(x => x.UserSchools).ThenInclude(x => x.School)
                        .Where(x => x.Id == idRecipient.Select(a => a.Id).FirstOrDefault())
                        .Select(x => new
                        {
                            idSchool = x.UserSchools.Select(a => a.IdSchool).FirstOrDefault(),
                            schoolName = x.UserSchools.Select(a => a.School).FirstOrDefault().Description
                        }).FirstOrDefault();

            var senderName = await _dbContext.Entity<MsUser>().Where(x => x.Id == trMessage.IdSender).Select(x => x.DisplayName).FirstOrDefaultAsync();

            paramTemplateEmailNotification.Add("idMessage", trMessage.Id);
            paramTemplateEmailNotification.Add("sendBy", senderName);
            paramTemplateEmailNotification.Add("typeMessage", trMessage.Type.ToString());

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                if (listIdRecipient.Count() > 1000)
                {
                    var chunkRecipients = listIdRecipient.ChunkBy(1000);

                    foreach (var chunkRecipient in chunkRecipients)
                    {
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS9")
                        {
                            IdSchool = school.idSchool,
                            IdRecipients = chunkRecipient,
                            KeyValues = paramTemplateEmailNotification
                        });
                        collector.Add(message);
                    }
                }
                else
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS9")
                    {
                        IdSchool = school.idSchool,
                        IdRecipients = listIdRecipient,
                        KeyValues = paramTemplateEmailNotification
                    });
                    collector.Add(message);
                }
            }
        }

        //send email to approver 2
        public async Task SendEmailToApproval2(TrMessage trMessage)
        {
            var checkApprover = _dbContext.Entity<MsMessageCategory>()
                                .Include(x => x.MessageApproval)
                                    .ThenInclude(x => x.ApprovalStates)
                                        .OrderBy(x => x.MessageApproval.ApprovalStates.FirstOrDefault().Number).FirstOrDefault(x => x.Id == trMessage.IdMessageCategory);

            if (checkApprover.MessageApproval.ApprovalStates.Count == 0)
            {
                return;
            }

            var idApprover2 = checkApprover.MessageApproval.ApprovalStates.Where(x => x.Number == 2).FirstOrDefault()?.IdUser;

            var approver = _dbContext.Entity<MsUser>()
                            .Include(x => x.UserSchools)
                            .Where(x => x.Id == idApprover2)
                            .Select(x => new
                            {
                                idRecipient = x.Id,
                                DisplayName = x.DisplayName,
                                idSchool = x.UserSchools.Select(a => a.IdSchool).FirstOrDefault()
                            }).FirstOrDefault();

            var senderName = await _dbContext.Entity<MsUser>().Where(x => x.Id == trMessage.IdSender).Select(x => x.DisplayName).FirstOrDefaultAsync();

            var messageCategory = await _dbContext.Entity<MsMessageCategory>().Where(x => x.Id == trMessage.IdMessageCategory).Select(x => x.Description).FirstOrDefaultAsync();

            var dateStartValue = trMessage.PublishStartDate == null ? "" : trMessage.PublishStartDate.Value.ToString("dd/MMMM/yyyy");
            var dateEndValue = trMessage.PublishEndDate == null ? "" : trMessage.PublishEndDate.Value.ToString("dd/MMMM/yyyy");

            paramTemplateEmailNotification.Add("id", trMessage.Id);
            paramTemplateEmailNotification.Add("approverName", approver.DisplayName);
            paramTemplateEmailNotification.Add("messageType", trMessage.Type.ToString());
            paramTemplateEmailNotification.Add("by", senderName);
            paramTemplateEmailNotification.Add("publishStartDate", dateStartValue);
            paramTemplateEmailNotification.Add("publishEndDate", dateEndValue);
            paramTemplateEmailNotification.Add("subject", trMessage.Subject);
            paramTemplateEmailNotification.Add("messageCategory", messageCategory);
            paramTemplateEmailNotification.Add("idSender", trMessage.IdSender);

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS4")
                {
                    IdSchool = approver.idSchool,
                    IdRecipients = new[] { approver.idRecipient },
                    KeyValues = paramTemplateEmailNotification
                });
                collector.Add(message);
            }
        }

        //send notif to student
        public async Task SendNotifToStudent(TrMessage trMessage, string idUser)
        {
            IDictionary<string, object> templateNotif = new Dictionary<string, object>();

            var messageRecepient = _dbContext.Entity<TrMessageRecepient>()
                                    .Include(x => x.Message)
                                    .Where(x => x.IdMessage == trMessage.Id && x.MessageFolder == MessageFolder.Inbox).ToList();

            var subjectMessage = trMessage.Subject;

            var sendByMessage = string.Empty;
            sendByMessage = !trMessage.IsSetSenderAsSchool
                ? await _dbContext.Entity<MsUser>().Where(x => x.Id == trMessage.IdSender).Select(x => x.DisplayName).FirstOrDefaultAsync(CancellationToken)
                : trMessage.IdSender == Guid.Empty.ToString()
                    ? "System"
                    : await _dbContext.Entity<MsUserSchool>()
                                                .Include(x => x.School)
                                                .Where(x => x.IdUser == trMessage.IdSender)
                                                .Select(x => x.School.Description).FirstOrDefaultAsync(CancellationToken);

            var school = _dbContext.Entity<MsUser>()
                            .Include(x => x.UserSchools).ThenInclude(x => x.School)
                            .Where(x => x.Id == idUser)
                            .Select(x => new
                            {
                                idSchool = x.UserSchools.Select(y => y.School.Id)
                            }).FirstOrDefault();

            templateNotif.Add("id", trMessage.Id);
            templateNotif.Add("messageType", trMessage.Type.ToString());
            templateNotif.Add("subject", subjectMessage);
            templateNotif.Add("sendBy", sendByMessage);

            if (messageRecepient.Count() > 1000)
            {
                var chunkRecipients = messageRecepient.Select(x => x.IdRecepient).ChunkBy(1000);

                foreach (var chunkRecipient in chunkRecipients)
                {
                    if (trMessage.Type == UserMessageType.Announcement || trMessage.Type == UserMessageType.Information)
                    {
                        if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                        {
                            var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS2")
                            {
                                IdRecipients = chunkRecipient,
                                KeyValues = templateNotif,
                            });
                            collector.Add(message);
                        }
                    }

                    if (trMessage.Type == UserMessageType.Private)
                    {
                        if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                        {
                            var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS7")
                            {
                                IdRecipients = chunkRecipient,
                                KeyValues = templateNotif
                            });
                            collector.Add(message);
                        }
                    }
                }
            }
            else
            {
                if (trMessage.Type == UserMessageType.Announcement || trMessage.Type == UserMessageType.Information)
                {
                    if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                    {
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS2")
                        {
                            IdRecipients = messageRecepient.Select(x => x.IdRecepient),
                            KeyValues = templateNotif,
                        });
                        collector.Add(message);
                    }
                }

                if (trMessage.Type == UserMessageType.Private)
                {
                    if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                    {
                        var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS7")
                        {
                            IdRecipients = messageRecepient.Select(x => x.IdRecepient),
                            KeyValues = templateNotif
                        });
                        collector.Add(message);
                    }
                }
            }
        }

        public async Task SendEmailApproveRejectToSender(TrMessage trMessage)
        {
            var staffTeacher = _dbContext.Entity<TrMessage>()
                               .Include(x => x.User).ThenInclude(x => x.UserSchools).ThenInclude(x => x.School)
                               .Where(x => x.Id == trMessage.Id)
                               .Select(x => new
                               {
                                   idSchool = x.User.UserSchools.Select(x => x.IdSchool).FirstOrDefault(),
                                   idStaffTeacher = x.IdSender,
                                   schoolName = x.User.UserSchools.Select(y => y.School.Description).FirstOrDefault(),
                               }).FirstOrDefault();

            var senderName = await _dbContext.Entity<MsUser>().Where(x => x.Id == trMessage.IdSender).Select(x => x.DisplayName).FirstOrDefaultAsync();

            var messageCategory = await _dbContext.Entity<MsMessageCategory>().Where(x => x.Id == trMessage.IdMessageCategory).Select(x => x.Description).FirstOrDefaultAsync();

            string requestDate = trMessage.DateIn == null ? string.Empty : trMessage.DateIn.GetValueOrDefault().ToString("dd MMMM yyyy");

            paramTemplateEmailNotification.Add("id", trMessage.Id);
            paramTemplateEmailNotification.Add("teacherName", senderName);
            paramTemplateEmailNotification.Add("messageType", trMessage.Type.ToString());
            paramTemplateEmailNotification.Add("by", senderName);
            paramTemplateEmailNotification.Add("subject", trMessage.Subject);
            paramTemplateEmailNotification.Add("messageCategory", messageCategory);
            paramTemplateEmailNotification.Add("idSender", trMessage.IdSender);
            paramTemplateEmailNotification.Add("status", trMessage.Status.ToString());
            paramTemplateEmailNotification.Add("requestDate", requestDate);

            if (trMessage.Status == StatusMessage.Rejected)
            {
                if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS5")
                    {
                        IdSchool = staffTeacher.idSchool,
                        IdRecipients = new[] { staffTeacher.idStaffTeacher },
                        KeyValues = paramTemplateEmailNotification
                    });
                    collector.Add(message);
                }
            }

            if (trMessage.Status == StatusMessage.Approved)
            {
                if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS6")
                    {
                        IdSchool = staffTeacher.idSchool,
                        IdRecipients = new[] { staffTeacher.idStaffTeacher },
                        KeyValues = paramTemplateEmailNotification
                    });
                    collector.Add(message);
                }
            }

        }
    }
}
