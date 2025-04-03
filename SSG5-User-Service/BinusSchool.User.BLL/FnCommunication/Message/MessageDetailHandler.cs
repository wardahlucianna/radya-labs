using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Utils;
using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;
using System.Text.Encodings.Web;
using System.Net;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.Persistence.UserDb.Entities.Student;
using Microsoft.OpenApi.Extensions;
using Microsoft.OData.Edm;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public MessageDetailHandler(
            IUserDbContext userDbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = userDbContext;
            _dateTime = dateTime;
        }

        private string FormatRecepients(List<TrMessageRecepient> trMessageRecepients)
        {
            return trMessageRecepients.Count == 0
                ? null
                : trMessageRecepients.Count >= 2
                    ? $"Send to: {trMessageRecepients.FirstOrDefault().User.DisplayName}, {trMessageRecepients.ElementAt(1).User.DisplayName} and {trMessageRecepients.Count - 2} others"
                    : trMessageRecepients.Count == 2
                        ? $"Send to: {trMessageRecepients.FirstOrDefault().User.DisplayName}, {trMessageRecepients.ElementAt(1).User.DisplayName}"
                        : $"Send to: {trMessageRecepients.FirstOrDefault().User.DisplayName}";
        }

        private string FormatAttachments(List<TrMessageAttachment> trMessageAttachments)
        {
            return trMessageAttachments.Count == 0
                ? null
                : trMessageAttachments.Count >= 2
                    ? $"{trMessageAttachments.FirstOrDefault().Url}, {trMessageAttachments.ElementAt(1).Url} and {trMessageAttachments.Count - 2} others"
                    : trMessageAttachments.Count == 2
                        ? $"{trMessageAttachments.FirstOrDefault().Url}, {trMessageAttachments.ElementAt(1).Url}"
                        : $"{trMessageAttachments.FirstOrDefault().Url}";

        }

        private bool GetIsRead(GetMessageDetailRequest param, TrMessage trMessage)
        {
            if (param.MessageFolder == MessageFolder.Sent) return true;

            var recepient = trMessage.MessageRecepients.Where(m => m.IdRecepient == param.UserId).FirstOrDefault();
            if (recepient == null)
                return false;

            return recepient.IsRead;
        }
        private string GetSenderName(TrMessage trMessage)
        {
            if (!trMessage.IsSetSenderAsSchool) return trMessage.User.DisplayName;

            if (trMessage.MessageParent != null && trMessage.MessageRecepients.Any(m => m.MessageFolder == MessageFolder.Inbox)) return trMessage.User.DisplayName;

            if (trMessage.User.Id == Guid.Empty.ToString())
                return "System";

            var school = trMessage.User.UserSchools.FirstOrDefault(x => x.IdUser == x.User.Id);
            if (school == null)
                throw new NotFoundException("School not found");

            return school.School.Description;
        }

        private string GetSubject(TrMessage trMessage)
        {
            if (trMessage.ParentMessageId != null) return trMessage.Subject;
            if (trMessage.Subject == null || trMessage.Subject == "") return "(No Subject)";
            return trMessage.Subject;
        }

        private string GetRecepients(GetMessageDetailRequest param, TrMessage trMessage)
        {
            if (trMessage.IsDraft) return FormatRecepients(trMessage.MessageRecepients.Where(x => x.IdRecepient == param.UserId).ToList());
            return null;
        }

        private string GetAttachments(GetMessageDetailRequest param, TrMessage trMessage)
        {
            if (trMessage.IsDraft) return FormatAttachments(trMessage.MessageAttachments.Where(x => x.IdMessage == param.IdMessage).ToList());
            return null;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMessageDetailRequest>(nameof(GetMessageDetailRequest.IdMessage), nameof(GetMessageDetailRequest.UserId));

            var predicate = PredicateBuilder.Create<TrMessage>(x => true);

            var query = _dbContext.Entity<TrMessage>()
                .Include(x => x.User)
                    .ThenInclude(x => x.UserSchools)
                        .ThenInclude(x => x.School)
                .Include(x => x.MessageRecepients)
                .Include(x => x.FeedbackType)
                .Include(x => x.MessageFors).ThenInclude(e => e.MessageForDepartements).ThenInclude(e => e.Department)
                .Include(x => x.MessageFors).ThenInclude(e => e.MessageForPositions).ThenInclude(e => e.TeacherPosition)
                .Include(x => x.MessageFors).ThenInclude(e => e.MessageForGrades).ThenInclude(e => e.Level)
                .Include(x => x.MessageFors).ThenInclude(e => e.MessageForGrades).ThenInclude(e => e.Grade)
                .Include(x => x.MessageFors).ThenInclude(e => e.MessageForGrades).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.MsClassroom)
                .Include(x => x.MessageFors).ThenInclude(e => e.MessageForPersonals).ThenInclude(e => e.User)
                .Include(x => x.MessageFors).ThenInclude(e => e.MessageForPersonals)
                .Where(predicate);

            var queryReply = _dbContext.Entity<TrMessage>()
                .Include(x => x.User)
                    .ThenInclude(x => x.UserSchools)
                        .ThenInclude(x => x.School)
                .Include(x => x.MessageRecepients)
                .Include(x => x.FeedbackType)
                .Where(x => x.ParentMessageId == param.IdMessage);

            // Message IdMessage Filter
            if (param.IdMessage != null || param.IdMessage != "")
                query = query.Where(x => x.Id == param.IdMessage);

            var datas = query.ToList();

            if (!datas.Any())
            {
                var dataIsDelete = query.IgnoreQueryFilters().ToList();

                if(dataIsDelete.Any())
                    throw new NotFoundException("This message has been deleted");
            }

            var trMessages = await query
                .OrderByDescending(x => x.IsMarkAsPinned)
                    .ThenByDescending(x => x.DateIn)
                .FirstOrDefaultAsync(CancellationToken);

            var trMessageReplies = await queryReply
                .OrderByDescending(x => x.IsMarkAsPinned)
                    .ThenByDescending(x => x.DateIn)
                .ToListAsync(CancellationToken);

            var queryPrevious = _dbContext.Entity<TrMessage>()
                .Include(x => x.User)
                    .ThenInclude(x => x.UserSchools)
                        .ThenInclude(x => x.School)
                .Include(x => x.MessageRecepients)
                .Where(x => (x.Id == param.IdMessage || ((trMessages.ParentMessageId != null ? ((x.ParentMessageId == trMessages.ParentMessageId && (x.IdSender == trMessages.IdSender || x.IdSender == param.UserId) || x.Id == trMessages.ParentMessageId)) : x.Id == param.IdMessage))) && x.DateIn < trMessages.DateIn);

            queryPrevious = queryPrevious.Where(x => x.MessageRecepients
                                                        .Any(m =>
                                                            (m.IdRecepient == param.UserId || m.IdRecepient == trMessages.IdSender) &&
                                                            m.MessageFolder == MessageFolder.Inbox
                                                        )
            );
            var trMessagePrevious = await queryPrevious
                .OrderByDescending(x => x.DateIn)
                .ToListAsync(CancellationToken);

            var queryAttachment = _dbContext.Entity<TrMessageAttachment>();

            var trMessageAttachment = await queryAttachment
            .Where(a => a.IdMessage == param.IdMessage)
            .OrderByDescending(a => a.DateIn)
            .ToListAsync(CancellationToken);

            var queryMessageCategory = await (from m in _dbContext.Entity<TrMessage>()
                                              join c in _dbContext.Entity<MsMessageCategory>() on m.IdMessageCategory equals c.Id into clf
                                              from xc in clf.DefaultIfEmpty()
                                              where m.Id == param.IdMessage
                                              select new MsMessageCategory
                                              {
                                                  Id = xc.Id,
                                                  Code = xc.Code,
                                                  Description = xc.Description
                                              }).FirstOrDefaultAsync(CancellationToken);

            var dataRecepient = await (from r in _dbContext.Entity<TrMessageRecepient>()
                                       where r.IdMessage == param.IdMessage && r.IdRecepient == param.UserId
                                       select new TrMessageRecepient
                                       {
                                           IdRecepient = r.IdRecepient,
                                           IdMessage = r.IdMessage,
                                           MessageFolder = r.MessageFolder,
                                           IsRead = r.IsRead
                                       }).ToListAsync(CancellationToken);

            var dataAttachment = await (from a in _dbContext.Entity<TrMessageAttachment>()
                                        where a.IdMessage == param.IdMessage
                                        select new TrMessageAttachment
                                        {
                                            Id = a.Id,
                                            IdMessage = a.IdMessage,
                                            Url = a.Url,
                                            Filename = a.Filename,
                                            Filetype = a.Filetype,
                                            Filesize = a.Filesize
                                        }).ToListAsync(CancellationToken);

            var dataAttachmentReply = await (from a in _dbContext.Entity<TrMessage>()
                                             join b in _dbContext.Entity<TrMessageAttachment>() on a.Id equals b.IdMessage
                                             where a.ParentMessageId == param.IdMessage
                                             select new TrMessageAttachment
                                             {
                                                 Id = b.Id,
                                                 IdMessage = b.IdMessage,
                                                 Url = b.Url,
                                                 Filename = b.Filename,
                                                 Filetype = b.Filetype,
                                                 Filesize = b.Filesize
                                             }).ToListAsync(CancellationToken);

            var dataAttachmentPrevious = await (from a in _dbContext.Entity<TrMessageAttachment>()
                                                select new TrMessageAttachment
                                                {
                                                    Id = a.Id,
                                                    IdMessage = a.IdMessage,
                                                    Url = a.Url,
                                                    Filename = a.Filename,
                                                    Filetype = a.Filetype,
                                                    Filesize = a.Filesize
                                                }).ToListAsync(CancellationToken);

            var listApproval = await (from a in _dbContext.Entity<TrMessageApproval>()
                                      where a.IdMessage == param.IdMessage
                                      orderby a.DateIn descending
                                      select new TrMessageApproval
                                      {
                                          IdMessage = a.IdMessage,
                                          StateNumber = a.StateNumber,
                                          IsApproved = a.IsApproved,
                                          IdRole = a.IdRole,
                                          IdUser = a.IdUser,
                                          Reason = a.Reason,
                                      }).ToListAsync(CancellationToken);

            var dataApprovalPrevious = await (from a in _dbContext.Entity<TrMessageApproval>()
                                              where a.IdMessage == param.IdMessage && a.StateNumber == 1
                                              orderby a.DateIn descending
                                              select new TrMessageApproval
                                              {
                                                  IdMessage = a.IdMessage,
                                                  StateNumber = a.StateNumber,
                                                  IsApproved = a.IsApproved,
                                                  IdRole = a.IdRole,
                                                  IdUser = a.IdUser,
                                                  Reason = a.Reason,
                                                  IsUnsendApproved = a.IsUnsendApproved,
                                                  UnsendReason = a.UnsendReason
                                              }).FirstOrDefaultAsync(CancellationToken);

            var dataApproval = await (from a in _dbContext.Entity<TrMessageApproval>()
                                      where a.IdMessage == param.IdMessage && a.StateNumber == 2
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
                                          IsUnsendApproved = a.IsUnsendApproved,
                                          UnsendReason = a.UnsendReason
                                      }).FirstOrDefaultAsync(CancellationToken);

            var dataApprovalUnsend = await (from a in _dbContext.Entity<TrMessageApproval>()
                                            where a.IdMessage == param.IdMessage && a.IsApproved && !a.IsUnsendApproved
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
                                                IsUnsendApproved = a.IsUnsendApproved,
                                                UnsendReason = a.UnsendReason
                                            }).FirstOrDefaultAsync(CancellationToken);

            var dataUser = await (from a in _dbContext.Entity<MsUser>()
                                  join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                  join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                  where a.Id == param.UserId
                                  orderby a.DateIn descending
                                  select new LtRole
                                  {
                                      Id = a.Id,
                                      IdRoleGroup = rg.IdRoleGroup,
                                  }).FirstOrDefaultAsync(CancellationToken);

            var haveApproval = await (from a in _dbContext.Entity<TrMessageApproval>()
                                      where a.IdMessage == param.IdMessage && a.IdUser == dataUser.Id && a.IsApproved == false && a.DateUp == null
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
                                          IsUnsendApproved = a.IsUnsendApproved,
                                          UnsendReason = a.UnsendReason
                                      }).FirstOrDefaultAsync(CancellationToken);

            var haveApprovalUnsend = await (from a in _dbContext.Entity<TrMessageApproval>()
                                            where a.IdMessage == param.IdMessage && a.IdUser == dataUser.Id && a.IsUnsendApproved == false && a.IsApproved == true
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
                                                IsUnsendApproved = a.IsUnsendApproved,
                                                UnsendReason = a.UnsendReason
                                            }).FirstOrDefaultAsync(CancellationToken);

            var approve_status = "";
            var approve_reason = "";

            var approve_previous_status = "";
            var approve_previous_reason = "";

            if (trMessages.IsUnsend)
            {
                if (dataApproval == null && dataApprovalUnsend == null)
                {
                    approve_status = "";
                    approve_reason = "";
                }
                else if (dataApproval != null && dataApprovalUnsend == null)
                {
                    if (trMessages.StatusUnsend == StatusMessage.Approved && dataApproval.StateNumber == 1 && dataApproval.IsUnsendApproved == true)
                    {
                        approve_status = "Unsend Approved";
                        approve_reason = dataApprovalPrevious.UnsendReason;
                    }
                    if (trMessages.StatusUnsend == StatusMessage.Approved && dataApproval.StateNumber == 2 && dataApproval.IsUnsendApproved == true)
                    {
                        approve_status = "Unsend Approved";
                        approve_reason = dataApproval.UnsendReason;
                        approve_previous_status = "Unsend Approved";
                        approve_previous_reason = dataApprovalPrevious.UnsendReason;
                    }
                    if ((trMessages.StatusUnsend == StatusMessage.Rejected || trMessages.StatusUnsend == StatusMessage.UnsendRejected)
                        && dataApproval.StateNumber == 1 && dataApproval.IsUnsendApproved == false)
                    {
                        approve_status = StatusMessage.UnsendRejected.GetDescription(); // set to unsend rejected
                        approve_reason = dataApprovalPrevious.UnsendReason;
                    }
                    if ((trMessages.StatusUnsend == StatusMessage.Rejected || trMessages.StatusUnsend == StatusMessage.UnsendRejected)
                        && dataApproval.StateNumber == 2 && dataApproval.IsUnsendApproved == false)
                    {
                        approve_status = StatusMessage.UnsendRejected.GetDescription(); // set to unsend rejected
                        approve_reason = dataApproval.UnsendReason;
                        approve_previous_status = "Unsend Approved";
                        approve_previous_reason = dataApprovalPrevious.UnsendReason;
                    }
                }
                else
                {
                    if (trMessages.StatusUnsend == StatusMessage.OnProgress && dataApprovalUnsend.StateNumber == 1 && dataApprovalUnsend.IsUnsendApproved == false)
                    {
                        approve_status = "Waiting Unsend Approval (" + dataApprovalUnsend.StateNumber + ")";
                        approve_reason = dataApprovalUnsend.UnsendReason;
                    }
                    if ((trMessages.StatusUnsend == StatusMessage.Rejected || trMessages.StatusUnsend == StatusMessage.UnsendRejected)
                        && dataApprovalUnsend.StateNumber == 1 && dataApprovalUnsend.IsUnsendApproved == false)
                    {
                        approve_status = StatusMessage.UnsendRejected.GetDescription(); // set to unsend rejected
                        approve_reason = dataApprovalUnsend.UnsendReason;
                    }
                    if (trMessages.StatusUnsend == StatusMessage.Approved && dataApprovalUnsend.StateNumber == 1 && dataApprovalUnsend.IsUnsendApproved == true)
                    {
                        approve_status = "Unsend Approved";
                        approve_reason = dataApprovalUnsend.UnsendReason;
                    }
                    if (trMessages.StatusUnsend == StatusMessage.Approved && dataApprovalUnsend.StateNumber == 2 && dataApprovalUnsend.IsUnsendApproved == true)
                    {
                        approve_status = "Unsend Approved";
                        approve_reason = dataApprovalUnsend.UnsendReason;
                        approve_previous_status = "Unsend Approved";
                        approve_previous_reason = dataApprovalPrevious.UnsendReason;
                    }
                    if (trMessages.StatusUnsend == StatusMessage.OnProgress && dataApprovalUnsend.StateNumber == 2 && dataApprovalUnsend.IsUnsendApproved == false)
                    {
                        approve_status = "Waiting Unsend Approval (" + dataApprovalUnsend.StateNumber + ")";
                        approve_previous_status = dataApprovalPrevious.IsUnsendApproved == true ? "Unsend Approved" : "Approved";
                        approve_previous_reason = dataApprovalPrevious.UnsendReason;
                    }
                    if ((trMessages.StatusUnsend == StatusMessage.Rejected || trMessages.StatusUnsend == StatusMessage.UnsendRejected)
                        && dataApprovalUnsend.StateNumber == 2 && dataApprovalUnsend.IsUnsendApproved == false)
                    {
                        approve_status = StatusMessage.UnsendRejected.GetDescription(); // set to unsend rejected
                        approve_reason = dataApprovalUnsend.UnsendReason;
                        approve_previous_status = "Unsend Approved";
                        approve_previous_reason = dataApprovalPrevious.UnsendReason;
                    }
                }
            }
            else
            {
                if (trMessages.Status == StatusMessage.Rejected)
                {
                    approve_status = "Rejected";
                    approve_reason = dataApproval != null ? dataApproval.Reason : "";
                }
                else if (trMessages.Status == StatusMessage.EditRejected)
                {
                    var previousMessage = await _dbContext.Entity<TrMessage>()
                        .Where(x => x.ParentMessageId == param.IdMessage)
                        .OrderByDescending(x => x.DateIn).Select(x => x.Id).FirstOrDefaultAsync();
                    if (previousMessage == null)
                        throw new NotFoundException("Previous message not found");

                    var lastDataApproval = await _dbContext.Entity<TrMessageApproval>()
                        .Where(x => x.IdMessage == previousMessage).OrderByDescending(x => x.DateIn).FirstOrDefaultAsync();
                    if (lastDataApproval == null)
                        throw new NotFoundException("Last approval data not found");

                    approve_status = "Edit Rejected";
                    approve_reason = lastDataApproval.Reason;
                }
                else
                {
                    if (dataApproval == null)
                    {
                        if (dataApprovalPrevious == null)
                        {
                            approve_status = "";
                            approve_reason = "";
                        }
                        else
                        {
                            if (dataApprovalPrevious.StateNumber == 1 && dataApprovalPrevious.IsApproved == false && dataApprovalPrevious.DateUp == null)
                            {
                                approve_status = "Waiting Approval (" + dataApprovalPrevious.StateNumber + ")";
                                approve_reason = dataApprovalPrevious.Reason;
                            }
                            if (dataApprovalPrevious.StateNumber == 1 && dataApprovalPrevious.IsApproved == false && dataApprovalPrevious.DateUp != null)
                            {
                                approve_status = "Rejected";
                                approve_reason = dataApprovalPrevious.Reason;
                            }
                            if (dataApprovalPrevious.StateNumber == 1 && dataApprovalPrevious.IsApproved == true)
                            {
                                approve_status = "Approved";
                                approve_reason = dataApprovalPrevious.Reason;
                            }
                        }
                    }
                    else
                    {
                        if (dataApproval.StateNumber == 1 && dataApproval.IsApproved == false && dataApproval.DateUp == null)
                        {
                            approve_status = "Waiting Approval (" + dataApproval.StateNumber + ")";
                            approve_reason = dataApproval.Reason;
                        }
                        if (dataApproval.StateNumber == 1 && dataApproval.IsApproved == false && dataApproval.DateUp != null)
                        {
                            approve_status = "Rejected";
                            approve_reason = dataApproval.Reason;
                        }
                        if (dataApproval.StateNumber == 1 && dataApproval.IsApproved == true)
                        {
                            approve_status = "Approved";
                            approve_reason = dataApproval.Reason;
                        }
                        if (dataApproval.StateNumber == 2 && dataApproval.IsApproved == true)
                        {
                            approve_status = "Approved";
                            approve_reason = dataApproval.Reason;
                            approve_previous_status = "Approved";
                            approve_previous_reason = dataApprovalPrevious.Reason;
                        }
                        if (dataApproval.StateNumber == 2 && dataApproval.IsApproved == false && dataApproval.DateUp == null)
                        {
                            approve_status = "Waiting Approval (" + dataApproval.StateNumber + ")";
                            approve_previous_status = dataApprovalPrevious.IsApproved == true ? "Approved" : "Rejected";
                            approve_previous_reason = dataApprovalPrevious.Reason;
                        }
                        if (dataApproval.StateNumber == 2 && dataApproval.IsApproved == false && dataApproval.DateUp != null)
                        {
                            approve_status = "Rejected";
                            approve_reason = dataApproval.Reason;
                            approve_previous_status = "Approved";
                            approve_previous_reason = dataApprovalPrevious.Reason;
                        }

                    }
                }
            }

            var isApproval = haveApproval != null ? true : false;
            var isApprovalUnsend = haveApprovalUnsend != null ? true : false;

            var getDataGroupMember = _dbContext.Entity<TrMessageGroupMember>()
                .Where(x => x.IdMessage == param.IdMessage).ToList();

            var GroupMember = new List<ListMemberSentTo>();

            if (getDataGroupMember.Any())
            {
                var listUser = getDataGroupMember.Where(e => e.GroupMailingList != null).Select(x => x.GroupMailingList.IdUser).ToList();

                var resultGroupIds = getDataGroupMember.Select(o => o.IdGroupMailingList).Distinct().ToList();

                var dataGroupMember = _dbContext.Entity<MsGroupMailingList>()
                    .Where(x => resultGroupIds.Any(y => y == x.Id))
                    .Select(x => new ListMemberSentTo
                    {
                        Id = x.Id,
                        GroupName = x.GroupName,
                        Description = x.Description
                    })
                .ToList();

                GroupMember.AddRange(dataGroupMember);
            }

            foreach (var data in GroupMember)
            {
                data.ListGroupMembers = GetMemberGroupMailings(param);
            }

            var res = new GetMessageDetailResult
            {
                CanEndConversation = trMessages.IsAllowReply && !trMessages.EndConversation,
                ReasonEndConversation = trMessages.ReasonEndConversation,
                ConversationDescription = trMessages.IsAllowReply ? !trMessages.EndConversation ? "Conversation Ongoing" : "Conversation Ended" : null,
                CanApprove = trMessages.IsUnsend ? isApprovalUnsend : isApproval,
                ApproveStatus = approve_status,
                ApproveReason = approve_reason,
                ApprovePreviousStatus = approve_previous_status,
                ApprovePreviousReason = approve_previous_reason,
                IdSender = trMessages.IsSetSenderAsSchool ? (trMessages.MessageParent != null && trMessages.MessageRecepients.Any(m => m.MessageFolder == MessageFolder.Inbox) ? trMessages.IdSender : null) : trMessages.IdSender,
                IsSetSenderAsSchool = trMessages.IsSetSenderAsSchool,
                Id = trMessages.Id,
                ParentMessageId = trMessages.ParentMessageId != null ? trMessages.ParentMessageId : trMessages.Id,
                Content = WebUtility.HtmlDecode(trMessages.Content),
                DateIn = trMessages.DateIn,
                FeedbackType = trMessages.IdFeedbackType != null ? trMessages.FeedbackType.Description : null,
                FeedbackTypeDetail = new CodeWithIdVm
                {
                    Id = trMessages.IdFeedbackType != null ? trMessages.FeedbackType.Id : null,
                    Code = trMessages.IdFeedbackType != null ? trMessages.FeedbackType.Code : null,
                    Description = trMessages.IdFeedbackType != null ? trMessages.FeedbackType.Description : null,
                },
                IsRead = GetIsRead(param, trMessages),
                PublishStartDate = trMessages.PublishStartDate,
                PublishEndDate = trMessages.PublishEndDate,
                ReplyStartDate = trMessages.ReplyStartDate,
                ReplyEndDate = trMessages.ReplyEndDate,
                SenderName = GetSenderName(trMessages),
                Subject = GetSubject(trMessages),
                Type = new CodeWithIdVm
                {
                    Id = trMessages.Type == UserMessageType.Private ? ((int)UserMessageType.Private).ToString() : trMessages.Type == UserMessageType.Announcement ? ((int)UserMessageType.Announcement).ToString() : trMessages.Type == UserMessageType.Information ? ((int)UserMessageType.Information).ToString() : ((int)UserMessageType.Feedback).ToString(),
                    Code = trMessages.Type == UserMessageType.Private ? ((int)UserMessageType.Private).ToString() : trMessages.Type == UserMessageType.Announcement ? ((int)UserMessageType.Announcement).ToString() : trMessages.Type == UserMessageType.Information ? ((int)UserMessageType.Information).ToString() : ((int)UserMessageType.Feedback).ToString(),
                    Description = trMessages.Type == UserMessageType.Private ? UserMessageType.Private.ToString() : trMessages.Type == UserMessageType.Announcement ? UserMessageType.Announcement.ToString() : trMessages.Type == UserMessageType.Information ? UserMessageType.Information.ToString() : UserMessageType.Feedback.ToString(),
                },
                IsAllowReply = trMessages.IsAllowReply,
                CanReplied = trMessages.Type == UserMessageType.Private ?
                                trMessages.ReplyEndDate.HasValue ?
                                    trMessages.IsAllowReply && !trMessages.EndConversation && (trMessages.ReplyStartDate.HasValue && _dateTime.ServerTime.Date >= trMessages.ReplyStartDate.Value.Date) && (trMessages.ReplyEndDate.HasValue && _dateTime.ServerTime.Date <= trMessages.ReplyEndDate.Value.Date) ?
                                        true : false
                                    : trMessages.IsAllowReply && !trMessages.EndConversation ?
                                        true : false
                                : trMessages.IsAllowReply && !trMessages.EndConversation ?
                                    true : false,
                IsMarkAsPinned = trMessages.IsMarkAsPinned,
                MessageCategory = new CodeWithIdVm
                {
                    Id = queryMessageCategory.Id,
                    Code = queryMessageCategory.Code,
                    Description = queryMessageCategory.Description
                },
                ProfilePicture = null,
                Recepients = dataRecepient.Select(x => new MessageRecepientResponse { IdRecepient = x.IdRecepient, IdMessage = x.IdMessage, MessageFolder = x.MessageFolder, IsRead = x.IsRead }).ToList(),
                Attachments = dataAttachment.Select(x => new MessageAttachmentResponse { Id = x.Id, IdMessage = x.IdMessage, Url = x.Url, Filename = x.Filename, Filetype = x.Filetype, Filesize = x.Filesize }).ToList(),
                Replies = trMessageReplies.Select(x => new GetMessageReplyResult
                {
                    IdSender = trMessages.IdSender,
                    SenderName = GetSenderName(x),
                    IsRead = GetIsRead(param, x),
                    Type = x.Type,
                    Subject = GetSubject(x),
                    DateIn = x.DateIn,
                    Content = WebUtility.HtmlDecode(x.Content),
                    IsMarkAsPinned = x.IsMarkAsPinned,
                    FeedbackType = x.IdFeedbackType != null ? x.FeedbackType.Description : null,
                    Attachments = dataAttachmentReply.Where(y => y.IdMessage == x.Id).Select(x => new MessageAttachmentResponse { Id = x.Id, IdMessage = x.IdMessage, Url = x.Url, Filename = x.Filename, Filetype = x.Filetype, Filesize = x.Filesize }).ToList()
                }).ToList(),
                Previous = trMessagePrevious.Count() < 1 ? null : trMessagePrevious.Select(y => new GetMessagePreviousResult
                {
                    IdSender = y.IsSetSenderAsSchool ? (y.MessageParent != null && y.MessageRecepients.Any(m => m.MessageFolder == MessageFolder.Inbox) ? y.IdSender : null) : y.IdSender,
                    SenderName = GetSenderName(y),
                    Subject = GetSubject(y),
                    DateIn = y.DateIn,
                    Content = WebUtility.HtmlDecode(y.Content),
                    ProfilePicture = null,
                    Attachments = dataAttachmentPrevious.Where(a => a.IdMessage == y.Id).Select(y => new MessageAttachmentResponse { Id = y.Id, IdMessage = y.IdMessage, Url = y.Url, Filename = y.Filename, Filetype = y.Filetype, Filesize = y.Filesize }).ToList()
                }).ToList(),
                IsSendEmail = trMessages.IsSendEmail,
                IsUnsend = trMessages.IsUnsend,
                StatusUnsend = trMessages.StatusUnsend.GetDescription(),
                IsGroupMember = getDataGroupMember.Any() ? true : false,
                GroupMember = GroupMember,
                MessageFors = trMessages.MessageFors
                .Select(e => new DetailMessageFor
                {
                    Depertements = e.MessageForDepartements
                                    .Select(f => new ItemValueVm
                                    {
                                        Id = f.Id,
                                        Description = f.Department.Description
                                    })
                                    .ToList(),
                    Role = e.Role.GetDescription(),
                    RoleEnum = e.Role,
                    Option = e.Option.GetDescription(),
                    OptionEnum = e.Option,
                    Positions = e.MessageForPositions
                                .Select(f => new ItemValueVm
                                {
                                    Id = f.IdTeacherPosition,
                                    Description = f.TeacherPosition.Description
                                })
                                .ToList(),
                    Personal = e.MessageForPersonals
                                .Select(f => new DetailMessageForPersonal
                                {
                                    IdUser = f.IdUser,
                                    UserName = f.User.DisplayName,
                                    FullName = f.User.Username,
                                    Role = e.Role.GetDescription()
                                })
                                .ToList(),
                    Grade = e.MessageForGrades
                            .GroupBy(e => new
                            {
                                IdLevel = e.Level.Id,
                                Level = e.Level.Description,
                                IdGrade = e.Grade == null ? null : e.Grade.Id,
                                Grade = e.Grade == null ? null : e.Grade.Description,
                                Semester = e.Semester == null ? null : e.Semester
                            })
                            .Select(f => new DetailMessageForGrade
                            {
                                Level = new ItemValueVm
                                {
                                    Id = f.Key.IdLevel,
                                    Description = f.Key.Level
                                },
                                Grade = new ItemValueVm
                                {
                                    Id = f.Key.IdGrade,
                                    Description = f.Key.Grade
                                },
                                Homeroom = f.Key.Grade == null
                                                ? null
                                                : e.MessageForGrades
                                                    .Where(d => d.IdGrade == f.Key.IdGrade && d.Semester == f.Key.Semester)
                                                    .GroupBy(d => new
                                                    {
                                                        GradeCode = d.Grade.Code,
                                                        ClassroomCode = d.Homeroom.GradePathwayClassroom.MsClassroom.Code,
                                                        IdHomeroom = d.Homeroom.Id
                                                    })
                                                    .Select(d => new ItemValueVm
                                                    {
                                                        Id = d.Key.IdHomeroom,
                                                        Description = d.Key.GradeCode + d.Key.ClassroomCode
                                                    }).ToList(),
                                Semester = f.Key.Semester
                            })
                            .ToList()
                }).ToList()
            };
            return Request.CreateApiResult2(res as object);
        }

        private List<ListUserSentTo> GetMemberGroupMailings(GetMessageDetailRequest param)
        {
            var currentAcademicYear = _dbContext.Entity<MsPeriod>()
              .Include(x => x.Grade)
                  .ThenInclude(x => x.MsLevel)
                      .ThenInclude(x => x.MsAcademicYear)
              .Where(x => x.Grade.MsLevel.MsAcademicYear.IdSchool == param.IdSchool)
              .Where(x => _dateTime.ServerTime.Date >= x.StartDate.Date)
              .Where(x => _dateTime.ServerTime.Date <= x.EndDate.Date)
              .Select(x => x.Grade.MsLevel.IdAcademicYear).FirstOrDefault();

            if (currentAcademicYear == null)
                throw new Exception("this date not contains in list academic year");

            var dataRecepient = (from a in _dbContext.Entity<TrMessageRecepient>()
                                 join u in _dbContext.Entity<MsUser>() on a.IdRecepient equals u.Id
                                 join r in _dbContext.Entity<MsUserRole>() on u.Id equals r.IdUser
                                 join lr in _dbContext.Entity<LtRole>() on r.IdRole equals lr.Id
                                 join lrg in _dbContext.Entity<LtRoleGroup>() on lr.IdRoleGroup equals lrg.Id
                                 join hs in _dbContext.Entity<MsHomeroomStudent>() on u.Id equals hs.IdStudent
                                 join h in _dbContext.Entity<MsHomeroom>() on hs.IdHomeroom equals h.Id
                                 join gpc in _dbContext.Entity<MsGradePathwayClassroom>() on h.IdGradePathwayClassRoom equals gpc.Id
                                 join c in _dbContext.Entity<MsClassroom>() on gpc.IdClassroom equals c.Id
                                 join gp in _dbContext.Entity<MsGradePathway>() on gpc.IdGradePathway equals gp.Id
                                 join g in _dbContext.Entity<MsGrade>() on gp.IdGrade equals g.Id
                                 join l in _dbContext.Entity<MsLevel>() on g.IdLevel equals l.Id
                                 join ay in _dbContext.Entity<MsAcademicYear>() on l.IdAcademicYear equals ay.Id
                                 join std in _dbContext.Entity<MsStudent>() on u.Id equals std.Id
                                 where a.IdMessage == param.IdMessage && a.MessageFolder == MessageFolder.Inbox
                                 && h.IdAcademicYear == currentAcademicYear
                                 select new ListUserSentTo
                                 {
                                     Id = a.Id,
                                     Role = lr.Description,
                                     Level = lrg.Id == "STD" ? l.Description : "-",
                                     Grade = lrg.Id == "STD" ? g.Description : "-",
                                     Homeroom = lrg.Id == "STD" ? g.Code + c.Code : "-",
                                     BinusianID = lrg.Id == "STD" ? std.Id : u.Id,
                                     Username = u.Username,
                                     FullName = lrg.Id == "STD" ? std.FirstName + " " + std.LastName : u.DisplayName,
                                 }).Distinct().ToList();

            var dataRecepientNonStudent = (from a in _dbContext.Entity<TrMessageRecepient>()
                                           join u in _dbContext.Entity<MsUser>() on a.IdRecepient equals u.Id
                                           join r in _dbContext.Entity<MsUserRole>() on u.Id equals r.IdUser
                                           join lr in _dbContext.Entity<LtRole>() on r.IdRole equals lr.Id
                                           join lrg in _dbContext.Entity<LtRoleGroup>() on lr.IdRoleGroup equals lrg.Id
                                           where a.IdMessage == param.IdMessage && a.MessageFolder == MessageFolder.Inbox
                                           select new ListUserSentTo
                                           {
                                               Id = a.Id,
                                               Role = lr.Description,
                                               Level = "-",
                                               Grade = "-",
                                               Homeroom = "-",
                                               BinusianID = u.Id,
                                               Username = u.Username,
                                               FullName = u.DisplayName,
                                           }).Distinct().ToList();

            var dataRecepientFilter = dataRecepient.Union(dataRecepientNonStudent).AsQueryable();

            dataRecepientFilter = dataRecepientFilter.GroupBy(x => x.BinusianID).Select(x => x.First()).AsQueryable();

            var dataRecepientFix = dataRecepientFilter
                .Select(x => new ListUserSentTo
                {
                    Id = x.Id,
                    Role = x.Role,
                    Level = x.Level,
                    Grade = x.Grade,
                    Homeroom = x.Homeroom,
                    BinusianID = x.BinusianID,
                    Username = x.Username,
                    FullName = x.FullName,
                }).ToList();

            return dataRecepientFix;
        }
    }
}
