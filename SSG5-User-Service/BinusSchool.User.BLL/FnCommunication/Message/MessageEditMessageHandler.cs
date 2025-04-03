using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnCommunication.Message.Validator;
using FluentEmail.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageEditMessageHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IRolePosition _apiRolePosition;
        private readonly IMessage _apiMessage;

        public MessageEditMessageHandler(IUserDbContext dbContext, IConfiguration configuration, IRolePosition apiRolePosition, IMessage apiMessage)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _apiRolePosition = apiRolePosition;
            _apiMessage = apiMessage;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddMessageRequest, AddMessageValidator>();

            var message = _dbContext.Entity<TrMessage>()
                .Include(x => x.MessageAttachments)
                .Include(X => X.MessageRecepients)
                .FirstOrDefault(x => x.Id == body.IdMessage);
            if (message == null)
                throw new NotFoundException("Message not found");

            message.Type = body.Type;
            message.IsSetSenderAsSchool = body.IsSetSenderAsSchool;
            message.Subject = body.Subject;
            message.Content = HtmlEncoder.Default.Encode(body.Content);
            message.IsMarkAsPinned = body.IsMarkAsPinned;
            message.IsSendEmail = body.IsSendEmail;

            if (body.Type == UserMessageType.Private)
            {
                message.ReplyStartDate = body.ReplyStartDate;
                message.ReplyEndDate = body.ReplyEndDate;
            }
            else if (body.Type == UserMessageType.Announcement || body.Type == UserMessageType.Information)
            {
                message.PublishStartDate = body.PublishStartDate;
                message.PublishEndDate = body.PublishEndDate;
            }

            if (body.Type != UserMessageType.AscTimetable && body.Type != UserMessageType.GenerateSchedule)
            {
                var messageCategory = _dbContext.Entity<MsMessageCategory>()
                    .Include(x => x.MessageApproval)
                        .ThenInclude(x => x.ApprovalStates)
                    .OrderBy(x => x.MessageApproval.ApprovalStates.FirstOrDefault().Number).FirstOrDefault(x => x.Id == body.IdMessageCategory);
                if (messageCategory != null)
                {
                    if (messageCategory.MessageApproval.ApprovalStates.Count > 0)
                    {
                        message.Status = StatusMessage.OnProgress;

                        _dbContext.Entity<TrMessageApproval>().Add(new TrMessageApproval
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdMessage = message.Id,
                            StateNumber = 1,
                            IdRole = messageCategory.MessageApproval.ApprovalStates.FirstOrDefault()?.IdRole,
                            IdUser = messageCategory.MessageApproval.ApprovalStates.FirstOrDefault()?.IdUser
                        });
                    }
                }
            }

            _dbContext.Entity<TrMessage>().Update(message);
            message.MessageAttachments.ForEach(e=>e.IsActive = false);
            _dbContext.Entity<TrMessageAttachment>().UpdateRange(message.MessageAttachments);

            _dbContext.Entity<TrMessageAttachment>().AddRange(body.Attachments.Select(x => new TrMessageAttachment
            {
                Id = Guid.NewGuid().ToString(),
                IdMessage = message.Id,
                Url = x.Url,
                Filename = x.Filename,
                Filetype = x.Filetype,
                Filesize = x.Filesize
            }));

            await _dbContext.SaveChangesAsync(CancellationToken);

            await _apiMessage.QueueNotificationMessage(new QueueMessagesRequest
            {
                IdSchool = body.IdSchool,
            });

            //await SendEmailNotif(trMessage, recepients);

            return Request.CreateApiResult2();
        }

        public async Task SendEmailNotif(TrMessage trMessage, List<string> recepients)
        {
            IDictionary<string, object> paramTemplateEmailNotification = new Dictionary<string, object>();

            var checkApprover = _dbContext.Entity<MsMessageCategory>()
                                .Include(x => x.MessageApproval)
                                    .ThenInclude(x => x.ApprovalStates)
                                        .OrderBy(x => x.MessageApproval.ApprovalStates.FirstOrDefault().Number).FirstOrDefault(x => x.Id == trMessage.IdMessageCategory);

            if (checkApprover.MessageApproval.ApprovalStates.Count == 0)
            {
                await SentNotifToRecipient(trMessage, recepients.FirstOrDefault());

                return;
            }

            var idApprover1 = checkApprover.MessageApproval.ApprovalStates.Where(x => x.Number == 1).FirstOrDefault()?.IdUser;

            var approver = _dbContext.Entity<MsUser>()
                            .Include(x => x.UserSchools)
                            .Where(x => x.Id == idApprover1)
                            .Select(x => new
                            {
                                idRecipient = x.Id,
                                DisplayName = x.DisplayName,
                                idSchool = x.UserSchools.Select(a => a.IdSchool).FirstOrDefault()
                            }).FirstOrDefault();

            var recipientName = await _dbContext.Entity<MsUser>().Where(x => recepients.Contains(x.Id)).Select(x => x.DisplayName).ToListAsync();

            var senderName = string.Empty;
            senderName = !trMessage.IsSetSenderAsSchool
                ? await _dbContext.Entity<MsUser>().Where(x => x.Id == trMessage.IdSender).Select(x => x.DisplayName).FirstOrDefaultAsync(CancellationToken)
                : trMessage.IdSender == Guid.Empty.ToString()
                    ? "System"
                    : await _dbContext.Entity<MsUserSchool>()
                                                .Include(x => x.School)
                                                .Where(x => x.IdUser == trMessage.IdSender)
                                                .Select(x => x.School.Description).FirstOrDefaultAsync(CancellationToken);



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
            paramTemplateEmailNotification.Add("recipient", string.Join(",", recipientName));

            if (trMessage.Type == UserMessageType.Announcement || trMessage.Type == UserMessageType.Information)
            {
                if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS1")
                    {
                        IdSchool = approver.idSchool,
                        IdRecipients = new[] { approver.idRecipient },
                        KeyValues = paramTemplateEmailNotification
                    });
                    collector.Add(message);
                }
            }

            if (trMessage.Type == UserMessageType.Private)
            {
                if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                {
                    var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS3")
                    {
                        IdSchool = approver.idSchool,
                        IdRecipients = new[] { approver.idRecipient },
                        KeyValues = paramTemplateEmailNotification
                    });
                    collector.Add(message);
                }
            }
        }

        public async Task SentNotifToRecipient(TrMessage trMessage, string idUser)
        {
            IDictionary<string, object> templateNotif = new Dictionary<string, object>();

            var messageRecepient = _dbContext.Entity<TrMessageRecepient>()
                                    .Include(x => x.Message)
                                    .Where(x => x.IdMessage == trMessage.Id && x.MessageFolder == MessageFolder.Inbox).ToList();

            var subjectMessage = trMessage.Subject;

            var sendByMessage = await _dbContext.Entity<MsUser>()
                        .Where(x => x.Id == trMessage.IdSender).Select(x => x.DisplayName).FirstOrDefaultAsync(CancellationToken);

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

            if (messageRecepient.Select(x => x.IdRecepient).Count() > 500)
            {
                var chunkRecipients = messageRecepient.Select(x => x.IdRecepient).ChunkBy(500);

                foreach (var chunkRecipient in chunkRecipients)
                {
                    if (trMessage.Type == UserMessageType.Announcement || trMessage.Type == UserMessageType.Information)
                    {
                        if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
                        {
                            var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "MSS2")
                            {
                                IdRecipients = chunkRecipient,
                                KeyValues = templateNotif
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
                            KeyValues = templateNotif
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

        #region Handler old
        //protected override async Task<ApiErrorResult<object>> Handler()
        //{
        //    var body = await Request.ValidateBody<AddMessageRequest, AddMessageValidator>();

        //    var message = _dbContext.Entity<TrMessage>()
        //        .Include(x => x.MessageAttachments)
        //        .Include(X => X.MessageRecepients)
        //        .FirstOrDefault(x => x.Id == body.IdMessage);
        //    if (message == null)
        //        throw new NotFoundException("Message not found");

        //    message.IsActive = false;
        //    _dbContext.Entity<TrMessage>().Update(message);

        //    var trMessage = new TrMessage();
        //    trMessage.Id = Guid.NewGuid().ToString();
        //    //trMessage.ParentMessageId = message.Id;
        //    trMessage.IsEdit = true;
        //    trMessage.IdSender = body.IdSender;
        //    trMessage.Type = body.Type;
        //    trMessage.IsSetSenderAsSchool = body.IsSetSenderAsSchool;
        //    trMessage.Subject = body.Subject;
        //    trMessage.IdMessageCategory = body.IdMessageCategory;
        //    trMessage.Content = HtmlEncoder.Default.Encode(body.Content);
        //    trMessage.IsMarkAsPinned = body.IsMarkAsPinned;
        //    trMessage.IsDraft = body.IsDraft;
        //    trMessage.Status = StatusMessage.Approved;
        //    trMessage.IsSendEmail = body.IsSendEmail;

        //    if (body.Type == UserMessageType.Private)
        //    {
        //        trMessage.IsAllowReply = body.IsAllowReply;
        //        trMessage.ReplyStartDate = body.ReplyStartDate;
        //        trMessage.ReplyEndDate = body.ReplyEndDate;
        //    }
        //    else if (body.Type == UserMessageType.Announcement || body.Type == UserMessageType.Information)
        //    {
        //        trMessage.PublishStartDate = body.PublishStartDate;
        //        trMessage.PublishEndDate = body.PublishEndDate;

        //        if (body.Type == UserMessageType.Information && !body.GroupMembers.Any())
        //        {
        //            // di takeout krn information tidak ada edit recepient
        //            //var trMrssageFor = trMessage.MessageFors.ToList();
        //            //if (trMrssageFor.Any())
        //            //{
        //            //    trMrssageFor.ForEach(e => e.IsActive = false);
        //            //    _dbContext.Entity<TrMessageFor>().UpdateRange(trMrssageFor);
        //            //}

        //            //var trMrssageForDepartement = trMessage.MessageFors.SelectMany(e => e.MessageForDepartements).ToList();
        //            //if (trMrssageForDepartement.Any())
        //            //{
        //            //    trMrssageForDepartement.ForEach(e => e.IsActive = false);
        //            //    _dbContext.Entity<TrMessageForDepartement>().UpdateRange(trMrssageForDepartement);
        //            //}

        //            //var trMrssageForPosition = trMessage.MessageFors.SelectMany(e => e.MessageForPositions).ToList();
        //            //if (trMrssageForPosition.Any())
        //            //{
        //            //    trMrssageForPosition.ForEach(e => e.IsActive = false);
        //            //    _dbContext.Entity<TrMessageForPosition>().UpdateRange(trMrssageForPosition);
        //            //}

        //            //var trMrssageForGrade = trMessage.MessageFors.SelectMany(e => e.MessageForGrades).ToList();
        //            //if (trMrssageForGrade.Any())
        //            //{
        //            //    trMrssageForGrade.ForEach(e => e.IsActive = false);
        //            //    _dbContext.Entity<TrMessageForGrade>().UpdateRange(trMrssageForGrade);
        //            //}

        //            //var trMrssageForPersonal = trMessage.MessageFors.SelectMany(e => e.MessageForPersonals).ToList();
        //            //if (trMrssageForPersonal.Any())
        //            //{
        //            //    trMrssageForPersonal.ForEach(e => e.IsActive = false);
        //            //    _dbContext.Entity<TrMessageForPersonal>().UpdateRange(trMrssageForPersonal);
        //            //}


        //            if (body.MessageFor.Any())
        //            {
        //                List<GetUserRolePosition> listUserRolePosition = new List<GetUserRolePosition>();

        //                GetUserRolePositionRequest paramGetUserRolePosition = new GetUserRolePositionRequest
        //                {
        //                    IdAcademicYear = body.IdAcademicYear,
        //                    IdSchool = body.IdSchool,
        //                };

        //                foreach (var item in body.MessageFor)
        //                {
        //                    var newMassageFor = new TrMessageFor
        //                    {
        //                        Id = Guid.NewGuid().ToString(),
        //                        Role = item.Role,
        //                        Option = item.Option,

        //                    };

        //                    _dbContext.Entity<TrMessageFor>().Add(newMassageFor);

        //                    if (item.Depertements.Any())
        //                    {
        //                        var newMassageForDepartement = item.Depertements
        //                            .Select(e => new TrMessageForDepartement
        //                            {
        //                                Id = Guid.NewGuid().ToString(),
        //                                IdDepartment = e,
        //                                IdMessageFor = newMassageFor.Id,
        //                            }).ToList();

        //                        _dbContext.Entity<TrMessageForDepartement>().AddRange(newMassageForDepartement);
        //                    }

        //                    if (item.TeacherPositions.Any())
        //                    {
        //                        var newMassageForPosition = item.TeacherPositions
        //                            .Select(e => new TrMessageForPosition
        //                            {
        //                                Id = Guid.NewGuid().ToString(),
        //                                IdTeacherPosition = e,
        //                                IdMessageFor = newMassageFor.Id,
        //                            }).ToList();

        //                        _dbContext.Entity<TrMessageForPosition>().AddRange(newMassageForPosition);
        //                    }

        //                    if (item.Grade.Any())
        //                    {
        //                        foreach (var itemGrade in item.Grade)
        //                        {
        //                            foreach (var idHomeroom in itemGrade.IdHomeroom)
        //                            {
        //                                var newMassageForGrade = new TrMessageForGrade
        //                                {
        //                                    Id = Guid.NewGuid().ToString(),
        //                                    IdLevel = itemGrade.IdLevel,
        //                                    IdGrade = itemGrade.IdGrade,
        //                                    IdHomeroom = idHomeroom,
        //                                    Semester = itemGrade.Semester,
        //                                    IdMessageFor = newMassageFor.Id,
        //                                };

        //                                _dbContext.Entity<TrMessageForGrade>().AddRange(newMassageForGrade);
        //                            }
        //                        }
        //                    }

        //                    if (item.Personal.Any())
        //                    {
        //                        var newMassageForPersonal = item.Personal
        //                            .Select(e => new TrMessageForPersonal
        //                            {
        //                                Id = Guid.NewGuid().ToString(),
        //                                IdUser = e,
        //                                IdMessageFor = newMassageFor.Id,
        //                            }).ToList();

        //                        _dbContext.Entity<TrMessageForPersonal>().AddRange(newMassageForPersonal);
        //                    }

        //                    UserRolePersonalOptionType option = UserRolePersonalOptionType.None;
        //                    if (item.Option == MessageForOption.None)
        //                        option = UserRolePersonalOptionType.None;
        //                    else if (item.Option == MessageForOption.All)
        //                        option = UserRolePersonalOptionType.All;
        //                    else if (item.Option == MessageForOption.Position)
        //                        option = UserRolePersonalOptionType.Position;
        //                    else if (item.Option == MessageForOption.Level)
        //                        option = UserRolePersonalOptionType.Level;
        //                    else if (item.Option == MessageForOption.Department)
        //                        option = UserRolePersonalOptionType.Department;
        //                    else if (item.Option == MessageForOption.Grade)
        //                        option = UserRolePersonalOptionType.Grade;
        //                    else if (item.Option == MessageForOption.PersonalUser)
        //                        option = UserRolePersonalOptionType.Personal;

        //                    var newUserRolePosition = new GetUserRolePosition
        //                    {
        //                        Role = item.Role,
        //                        Option = option,
        //                        Departemens = item.Depertements,
        //                        TeacherPositions = item.TeacherPositions,
        //                        Level = item.Grade.Select(e => e.IdLevel).Distinct().ToList(),
        //                        Homeroom = item.Grade.Where(e => e.IdHomeroom != null).SelectMany(e => e.IdHomeroom).Distinct().ToList(),
        //                        Personal = item.Personal
        //                    };
        //                    listUserRolePosition.Add(newUserRolePosition);
        //                }

        //                var apiGetUserRolePosition = await _apiRolePosition.GetUserRolePosition(paramGetUserRolePosition);
        //                var GetUserRolePosition = apiGetUserRolePosition.IsSuccess ? apiGetUserRolePosition.Payload : null;
        //                body.Recepients = GetUserRolePosition.Select(e => e.IdUser).Distinct().ToList();
        //            }
        //        }
        //    }

        //    if (body.GroupMembers.Any())
        //    {
        //        body.Recepients = await _dbContext.Entity<MsGroupMailingListMember>()
        //                            .Where(e => body.GroupMembers.Contains(e.IdGroupMailingList))
        //                            .Select(e => e.IdUser)
        //                            .Distinct()
        //                            .ToListAsync(CancellationToken);
        //    }

        //    var recepients = body.Recepients;
        //    var attachments = body.Attachments;

        //    _dbContext.Entity<TrMessage>().Add(trMessage);

        //    if (body.Type != UserMessageType.AscTimetable && body.Type != UserMessageType.GenerateSchedule)
        //    {
        //        var messageCategory = _dbContext.Entity<MsMessageCategory>()
        //            .Include(x => x.MessageApproval)
        //                .ThenInclude(x => x.ApprovalStates)
        //            .OrderBy(x => x.MessageApproval.ApprovalStates.FirstOrDefault().Number).FirstOrDefault(x => x.Id == body.IdMessageCategory);
        //        if (messageCategory != null)
        //        {
        //            if (messageCategory.MessageApproval.ApprovalStates.Count > 0)
        //            {
        //                trMessage.Status = StatusMessage.OnProgress;

        //                _dbContext.Entity<TrMessageApproval>().Add(new TrMessageApproval
        //                {
        //                    Id = Guid.NewGuid().ToString(),
        //                    IdMessage = trMessage.Id,
        //                    StateNumber = 1,
        //                    IdRole = messageCategory.MessageApproval.ApprovalStates.FirstOrDefault()?.IdRole,
        //                    IdUser = messageCategory.MessageApproval.ApprovalStates.FirstOrDefault()?.IdUser
        //                });
        //            }
        //        }
        //    }

        //    _dbContext.Entity<TrMessageRecepient>().Add(new TrMessageRecepient
        //    {
        //        Id = Guid.NewGuid().ToString(),
        //        IdRecepient = trMessage.IdSender,
        //        IdMessage = trMessage.Id,
        //        MessageFolder = MessageFolder.Sent,
        //        IsRead = true
        //    });

        //    var removeRecepient = trMessage.MessageRecepients.Where(e => !recepients.Contains(e.IdRecepient)).ToList();
        //    if (removeRecepient.Any())
        //    {
        //        removeRecepient.ForEach(e => e.IsActive = false);
        //        _dbContext.Entity<TrMessageRecepient>().UpdateRange(removeRecepient);
        //    }

        //    var listIdRecepient = trMessage.MessageRecepients.Select(e => e.IdRecepient).Distinct().ToList();
        //    var addRecepient = recepients
        //                        .Where(e => !listIdRecepient.Contains(e))
        //                        .Select(x => new TrMessageRecepient
        //                        {
        //                            Id = Guid.NewGuid().ToString(),
        //                            IdRecepient = x,
        //                            IdMessage = trMessage.Id,
        //                            MessageFolder = MessageFolder.Inbox,
        //                            IsRead = false
        //                        })
        //                        .ToList();
        //    if (addRecepient.Any())
        //        _dbContext.Entity<TrMessageRecepient>().AddRange(addRecepient);

        //    _dbContext.Entity<TrMessageAttachment>().AddRange(attachments.Select(x => new TrMessageAttachment
        //    {
        //        Id = Guid.NewGuid().ToString(),
        //        IdMessage = trMessage.Id,
        //        Url = x.Url,
        //        Filename = x.Filename,
        //        Filetype = x.Filetype,
        //        Filesize = x.Filesize
        //    }));

        //    //await _dbContext.SaveChangesAsync(CancellationToken);

        //    //await _apiMessage.QueueNotificationMessage(new QueueMessagesRequest
        //    //{
        //    //    IdSchool = body.IdSchool,
        //    //});

        //    //await SendEmailNotif(trMessage, recepients);

        //    return Request.CreateApiResult2();
        //}
        #endregion
    }
}
