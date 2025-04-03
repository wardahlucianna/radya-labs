using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Net;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MessageHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public MessageHandler(
            IUserDbContext userDbContext,
            IMachineDateTime dateTime)
        {
            _dbContext = userDbContext;
            _dateTime = dateTime;
        }

        #region Old Function
        private string GetRecepients(List<TrMessageRecepient> trMessageRecepients)
        {
            return trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ToList().Count <= 0
                ? null
                : trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ToList().Count == 1
                    ? $"Sent to: {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).FirstOrDefault().User.DisplayName}"
                    : trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ToList().Count == 2
                        ? $"Sent to: {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).FirstOrDefault().User.DisplayName}, {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ElementAt(1).User.DisplayName}"
                        : $"Sent to: {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).FirstOrDefault().User.DisplayName}, {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ElementAt(1).User.DisplayName} and {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ToList().Count - 2} others";
        }

        private bool GetIsRead(GetMessageRequest param, TrMessage trMessage)
        {
            if (param.MessageFolder == MessageFolder.Sent) return true;

            var recepient = trMessage.MessageRecepients.Where(m => m.IdRecepient == param.UserId && m.MessageFolder == param.MessageFolder).FirstOrDefault();
            if (recepient == null)
                return false;

            return recepient.IsRead;
        }
        private string GetSenderName(TrMessage trMessage)
        {
            if (!trMessage.IsSetSenderAsSchool) return trMessage.User.DisplayName;

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
        private string GetApprovalStatus(TrMessage trMessage)
        {
            if (trMessage.Status != StatusMessage.OnProgress)
                return trMessage.Status.ToString();

            var messageApproval = trMessage.MessageApprovals.Where(x => !x.IsApproved).FirstOrDefault();
            if (messageApproval == null)
                return "Message approval not found";

            return "Waiting Approval (" + messageApproval.StateNumber + ")";
        }

        private string GetUnsendApprovalStatus(TrMessage trMessage)
        {
            if (trMessage.StatusUnsend != StatusMessage.OnProgress)
                return "Unsend " + trMessage.StatusUnsend.ToString();

            var messageApproval = trMessage.MessageApprovals.Where(x => x.IsApproved && !x.IsUnsendApproved).OrderBy(x => x.DateIn).FirstOrDefault();
            if (messageApproval == null)
                return "Message unsend approval not found";

            return "Waiting Unsend Approval (" + messageApproval.StateNumber + ")";
        }
        #endregion
        private string GetRecepientsV2(List<GetMessageRecepientTemp> trMessageRecepients)
        {
            return trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ToList().Count <= 0
                ? null
                : trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ToList().Count == 1
                    ? $"Sent to: {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).FirstOrDefault().DisplayName}"
                    : trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ToList().Count == 2
                        ? $"Sent to: {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).FirstOrDefault().DisplayName}, {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ElementAt(1).DisplayName}"
                        : $"Sent to: {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).FirstOrDefault().DisplayName}, {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ElementAt(1).DisplayName} and {trMessageRecepients.Where(x => x.MessageFolder == MessageFolder.Inbox).ToList().Count - 2} others";
        }

        private bool GetIsReadV2(GetMessageRequest param, string idMessage, List<GetMessageRecepientTemp> messageRecepients)
        {
            if (param.MessageFolder == MessageFolder.Sent) return true;

            var messageRecepient = messageRecepients.FirstOrDefault(m => m.IdRecepient == param.UserId && m.IdMessage == idMessage && m.MessageFolder == param.MessageFolder);
            if (messageRecepient == null)
                return false;
            else
                return messageRecepient.IsRead;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMessageRequest>(nameof(GetMessageRequest.MessageFolder), nameof(GetMessageRequest.UserId));

            var predicate = PredicateBuilder.Create<TrMessage>(x => true);

            if (!string.IsNullOrWhiteSpace(param.Search) && param.MessageFolder != MessageFolder.Inbox)
                predicate = predicate.And(x
                    => x.MessageRecepients.Any(r => r.User.DisplayName.Contains(param.Search)
                    || EF.Functions.Like(x.User.DisplayName, $"%{param.Search}%")
                    || EF.Functions.Like(x.Subject, $"%{param.Search}%")));

            var query = _dbContext.Entity<TrMessage>()
                .Include(x => x.User)
                    .ThenInclude(x => x.UserSchools)
                        .ThenInclude(x => x.School)
                .Include(x => x.MessageRecepients)
                    .ThenInclude(x => x.User)
                .Include(x => x.MessageCategory)
                    .ThenInclude(x => x.MessageApproval)
                        .ThenInclude(x => x.ApprovalStates)
                .Include(x => x.FeedbackType)
                .Include(x => x.MessageApprovals)
                .Where(predicate);

            if (param.Type == null)
            {
                var CheckRole = await (from a in _dbContext.Entity<MsUser>()
                                       join r in _dbContext.Entity<MsUserRole>() on a.Id equals r.IdUser
                                       join rg in _dbContext.Entity<LtRole>() on r.IdRole equals rg.Id
                                       where a.Id == param.UserId

                                       select new GetUserBySpecificFilterResult
                                       {
                                           Role = rg.IdRoleGroup
                                       }).FirstOrDefaultAsync(CancellationToken);
                if (CheckRole.Role == "STD" || CheckRole.Role == "PRT")
                {
                }
                else
                {
                    query = query.Where(x => x.Type != UserMessageType.Feedback);
                }
            }

            if (param.Type.HasValue)
            {
                query = query.Where(x => x.Type == param.Type.Value);

                if (param.Type == UserMessageType.Information)
                    query = query.Where(x => x.PublishEndDate.Value.Date >= _dateTime.ServerTime.Date);
            }

            if (param.IsRead != null)
            {
                if (param.IsRead == "1")
                {
                    query = query.Where(x => x.MessageRecepients
                        .Any(m =>
                            m.IdRecepient == param.UserId &&
                            m.IsRead == true
                        )
                    );
                }
                else
                {
                    query = query.Where(x => x.MessageRecepients
                        .Any(m =>
                            m.IdRecepient == param.UserId &&
                            m.IsRead == false
                        )
                    );
                }
            }

            if (param.FeedbackType != null)
                query = query.Where(x => x.IdFeedbackType == param.FeedbackType);

            if (!string.IsNullOrEmpty(param.IdMessageCategory))
                query = query.Where(x => x.IdMessageCategory == param.IdMessageCategory);

            if (param.ApprovalStatus.HasValue)
            {
                if (param.ApprovalStatus == StatusMessage.WaitingApprove1)
                {
                    query = query.Where(x => x.Status == StatusMessage.OnProgress && x.IsUnsend == false && x.MessageApprovals.Where(y => !y.IsApproved).FirstOrDefault().StateNumber == 1);
                }
                else if (param.ApprovalStatus == StatusMessage.WaitingApprove2)
                {
                    query = query.Where(x => x.Status == StatusMessage.OnProgress && x.IsUnsend == false && x.MessageApprovals.Where(y => !y.IsApproved).FirstOrDefault().StateNumber == 2);
                }
                else if (param.ApprovalStatus == StatusMessage.Approved)
                {
                    query = query.Where(x => x.Status == StatusMessage.Approved && x.IsUnsend == false);
                }
                else if (param.ApprovalStatus == StatusMessage.Rejected)
                {
                    query = query.Where(x => x.Status == StatusMessage.Rejected);
                }
                else if (param.ApprovalStatus == StatusMessage.WaitingUnsendApprove1)
                {
                    query = query.Where(x => x.IsUnsend == true && x.MessageApprovals.Where(y => y.IsApproved && !y.IsUnsendApproved).FirstOrDefault().StateNumber == 1);
                }
                else if (param.ApprovalStatus == StatusMessage.WaitingUnsendApprove2)
                {
                    query = query.Where(x => x.IsUnsend == true && x.MessageApprovals.Where(y => y.IsApproved && !y.IsUnsendApproved).FirstOrDefault().StateNumber == 2);
                }
                else if (param.ApprovalStatus == StatusMessage.EditRejected)
                {
                    query = query.Where(x => x.Status == StatusMessage.EditRejected);
                }
                else if (param.ApprovalStatus == StatusMessage.UnsendRejected)
                {
                    query = query.Where(x => x.Status == StatusMessage.UnsendRejected);
                }
                else
                { }
            }

            if (param.IsDraft)
            {
                query = query.Where(x =>
                    x.IdSender == param.UserId &&
                    x.IsDraft == true &&
                    x.MessageRecepients
                        .Any(m =>
                            m.IdRecepient == param.UserId &&
                            m.MessageFolder == param.MessageFolder
                        )
                );
            }
            else if (param.IsApproval)
            {
                var user = _dbContext.Entity<MsUser>()
                    .Include(x => x.UserSchools)
                    .Include(x => x.UserRoles)
                    .FirstOrDefault(x => x.Id == param.UserId);
                if (user == null)
                    throw new NotFoundException("User not found");

                var school = user.UserSchools.Where(x => x.IdUser == user.Id).FirstOrDefault();
                if (school == null)
                    throw new NotFoundException("School not found");

                var role = user.UserRoles.Where(x => x.IdUser == param.UserId).FirstOrDefault();
                if (role == null)
                    throw new NotFoundException("Role not found");

                query = query.Where(x =>
                    x.MessageCategory.MessageApproval.IdSchool == school.IdSchool &&
                    x.MessageCategory.MessageApproval.ApprovalStates.Any(y => y.IdUser == param.UserId) &&
                    //x.MessageRecepients.Any(y => y.MessageFolder == MessageFolder.Sent) &&
                    (x.ParentMessageId == null || (x.ParentMessageId != null && x.IsEdit)) &&
                    x.IsDraft == false
                // x.MessageApprovals.Where(y => y.IdMessage == x.Id).OrderByDescending(x => x.DateIn).FirstOrDefault().IdUser == param.UserId
                // x.Status == StatusMessage.OnProgress
                );
            }
            else if (param.MessageFolder == MessageFolder.Sent)
            {
                query = query.Where(x =>
                    x.IdSender == param.UserId &&
                    x.IsDraft == false &&
                    x.StatusUnsend != StatusMessage.Approved &&
                    x.MessageRecepients
                        .Any(m =>
                            m.IdRecepient == param.UserId &&
                            m.MessageFolder == param.MessageFolder
                        )
                    );

                if (_dbContext.Entity<MsUserRole>().Any(x => x.IdUser == param.UserId))
                {
                    var user = _dbContext.Entity<MsUser>()
                        .Include(x => x.UserRoles)
                        .FirstOrDefault(x => x.Id == param.UserId);
                    if (user == null)
                        throw new NotFoundException("User not found");

                    var role = user.UserRoles.Where(x => x.IdUser == user.Id).FirstOrDefault();
                    if (role == null)
                        throw new NotFoundException("Role not found");

                    // if ((!param.Type.HasValue && role.IdRole != "STD") || (param.Type.HasValue && param.Type.Value != UserMessageType.Feedback))
                    //     query = query.Where(x => x.Type != UserMessageType.Feedback); 
                }
            }
            else if (param.MessageFolder == MessageFolder.Trash)
            {
                query = query.Where(x =>
                    (x.Type == UserMessageType.Announcement ?
                     (!x.PublishEndDate.HasValue || (x.PublishEndDate.HasValue && x.PublishEndDate.Value.Date < _dateTime.ServerTime.Date) || x.MessageRecepients.Any(m =>
                        m.IdRecepient == param.UserId && m.MessageFolder == MessageFolder.Trash
                        && x.IsDraft == false
                    )) : true)
                    && x.MessageRecepients.Any(m =>
                        m.IdRecepient == param.UserId &&
                        (x.Type == UserMessageType.Announcement ? m.IdRecepient == param.UserId && (m.MessageFolder == MessageFolder.Trash || m.MessageFolder == MessageFolder.Inbox) : m.MessageFolder == MessageFolder.Trash)
                        && x.IsDraft == false
                    )
                );
            }
            else if (param.MessageFolder == MessageFolder.Unsend)
            {
                query = query.Where(x =>
                    x.IdSender == param.UserId &&
                    x.IsDraft == false &&
                    x.IsUnsend == true && (x.StatusUnsend == StatusMessage.Approved || x.StatusUnsend == StatusMessage.OnProgress) &&
                    x.MessageRecepients
                        .Any(m =>
                            m.IdRecepient == param.UserId &&
                            m.MessageFolder == param.MessageFolder
                        )
                    );

                if (_dbContext.Entity<MsUserRole>().Any(x => x.IdUser == param.UserId))
                {
                    var user = _dbContext.Entity<MsUser>()
                        .Include(x => x.UserRoles)
                        .FirstOrDefault(x => x.Id == param.UserId);
                    if (user == null)
                        throw new NotFoundException("User not found");

                    var role = user.UserRoles.Where(x => x.IdUser == user.Id).FirstOrDefault();
                    if (role == null)
                        throw new NotFoundException("Role not found");
                }
            }
            else
            {
                query = query.Where(x =>
                    x.MessageRecepients.Any(m =>
                        m.IdRecepient == param.UserId &&
                        m.MessageFolder == param.MessageFolder &&
                        x.IsDraft == false &&
                        x.StatusUnsend != StatusMessage.Approved &&
                        x.Status != StatusMessage.OnProgress &&
                        x.Status != StatusMessage.Rejected
                    )
                );

                if (_dbContext.Entity<MsUserRole>().Any(x => x.IdUser == param.UserId))
                {
                    var user = _dbContext.Entity<MsUser>()
                        .Include(x => x.UserRoles)
                        .FirstOrDefault(x => x.Id == param.UserId);
                    if (user == null)
                        throw new NotFoundException("User not found");

                    var role = user.UserRoles.Where(x => x.IdUser == user.Id).FirstOrDefault();
                    if (role == null)
                        throw new NotFoundException("Role not found");

                    // if ((!param.Type.HasValue && role.IdRole != "STD" && param.MessageFolder == MessageFolder.Trash) || (param.Type.HasValue && param.Type.Value != UserMessageType.Feedback))
                    //     query = query.Where(x => x.Type != UserMessageType.Feedback);   
                }
            }

            var trMessageRecepients = param.MessageFolder == MessageFolder.Inbox
                ? param.IsApproval ? await query.OrderByDescending(x => x.DateIn)
                                    .SelectMany(y => y.MessageRecepients.Select(z => new GetMessageRecepientTemp
                                    {
                                        IdMessage = z.IdMessage,
                                        IdRecepient = z.IdRecepient,
                                        MessageFolder = z.MessageFolder,
                                        DisplayName = z.User.DisplayName,
                                        IsRead = z.IsRead,
                                    })).ToListAsync(CancellationToken)
                : await query
                    .Where(x => (!x.PublishStartDate.HasValue || (x.PublishStartDate.HasValue && x.PublishStartDate.Value.Date <= _dateTime.ServerTime.Date))
                                && (x.Type == UserMessageType.Announcement ? (!x.PublishEndDate.HasValue || (x.PublishEndDate.HasValue && x.PublishEndDate.Value.Date >= _dateTime.ServerTime.Date)) : true)
                                && (param.Type.HasValue ? x.Type == param.Type : x.Type != UserMessageType.Information))
                    .OrderBy(x => x.MessageRecepients.Where(y => y.IdRecepient == param.UserId).FirstOrDefault().IsRead)
                        .ThenByDescending(x => x.IsMarkAsPinned)
                            .ThenByDescending(x => x.DateIn)
                    .SelectMany(y => y.MessageRecepients.Select(z => new GetMessageRecepientTemp
                    {
                        IdMessage = z.IdMessage,
                        IdRecepient = z.IdRecepient,
                        MessageFolder = z.MessageFolder,
                        DisplayName = z.User.DisplayName,
                        IsRead = z.IsRead,
                    })).ToListAsync(CancellationToken)
                : await query
                    .Where(x => (param.Type.HasValue ? x.Type == param.Type : param.MessageFolder == MessageFolder.Sent || param.MessageFolder == MessageFolder.Trash || param.MessageFolder == MessageFolder.Unsend ? true : x.Type != UserMessageType.Information))
                    .OrderByDescending(x => x.DateIn)
                    .SelectMany(y => y.MessageRecepients.Select(z => new GetMessageRecepientTemp
                    {
                        IdMessage = z.IdMessage,
                        IdRecepient = z.IdRecepient,
                        MessageFolder = z.MessageFolder,
                        DisplayName = z.User.DisplayName,
                        IsRead = z.IsRead,
                    })).ToListAsync(CancellationToken);

            var trMessages = param.MessageFolder == MessageFolder.Inbox
                ? param.IsApproval ? await query
                            //.OrderBy(x => x.MessageRecepients.Where(y => y.IdRecepient == param.UserId).FirstOrDefault().IsRead)
                            //    .ThenByDescending(x => x.IsMarkAsPinned)
                            .OrderByDescending(x => x.DateIn)
                            .Select(x => new GetMessageResult
                            {
                                EndConversation = x.EndConversation,
                                IdSender = x.IsSetSenderAsSchool ? null : x.IdSender,
                                Id = x.Id,
                                Content = WebUtility.HtmlDecode(x.Content),
                                DateIn = x.DateIn,
                                FeedbackType = x.Type == UserMessageType.Feedback ? x.FeedbackType.Description : null,
                                PublishEndDate = x.PublishEndDate,
                                PublishStartDate = x.PublishStartDate,
                                UserDisplayName = x.User.DisplayName ?? null,
                                SchoolDescription = x.User.UserSchools.Any(y => y.IdUser == x.User.Id) ? x.User.UserSchools.FirstOrDefault(y => y.IdUser == x.User.Id).School.Description : "System",
                                SenderName = !x.IsSetSenderAsSchool ? x.User.DisplayName : null,
                                Subject = !string.IsNullOrEmpty(x.ParentMessageId) ? x.Subject :
                                    !string.IsNullOrEmpty(x.Subject) ? x.Subject : "(No Subject)",
                                Type = x.Type,
                                IsMarkAsPinned = x.IsMarkAsPinned,
                                ProfilePicture = null,
                                ApprovalStatus = x.Status != StatusMessage.OnProgress ? x.Status.ToString() :
                                            x.MessageApprovals.Any(y => !y.IsApproved) ? $"Waiting Approval ({x.MessageApprovals.Where(y => !y.IsApproved).FirstOrDefault().StateNumber})" :
                                                "Message approval not found",
                                IsUnsend = x.IsUnsend,
                                UnsendApprovalStatus = x.StatusUnsend == StatusMessage.Approved ? $"Unsend {StatusMessage.Approved}" :
                                                    x.StatusUnsend == StatusMessage.UnsendRejected ? $"{StatusMessage.UnsendRejected.GetDescription()}" :
                                                        x.MessageApprovals.Any(y => y.IsApproved && !y.IsUnsendApproved) ? $"Waiting Unsend Approval ({x.MessageApprovals.Where(y => y.IsApproved && !y.IsUnsendApproved).OrderBy(y => y.DateIn).FirstOrDefault().StateNumber})" :
                                                            "Message unsend approval not found"
                            })
                    .ToListAsync(CancellationToken)
                : await query
                    .Where(x => (!x.PublishStartDate.HasValue || (x.PublishStartDate.HasValue && x.PublishStartDate.Value.Date <= _dateTime.ServerTime.Date))
                                && (x.Type == UserMessageType.Announcement ? (!x.PublishEndDate.HasValue || (x.PublishEndDate.HasValue && x.PublishEndDate.Value.Date >= _dateTime.ServerTime.Date)) : true)
                                && (param.Type.HasValue ? x.Type == param.Type : x.Type != UserMessageType.Information))
                    .OrderBy(x => x.MessageRecepients.Where(y => y.IdRecepient == param.UserId).FirstOrDefault().IsRead)
                        .ThenByDescending(x => x.IsMarkAsPinned)
                            .ThenByDescending(x => x.DateIn)
                    .Select(x => new GetMessageResult
                    {
                        EndConversation = x.EndConversation,
                        IdSender = x.IsSetSenderAsSchool ? null : x.IdSender,
                        Id = x.Id,
                        Content = WebUtility.HtmlDecode(x.Content),
                        DateIn = x.DateIn,
                        FeedbackType = x.Type == UserMessageType.Feedback ? x.FeedbackType.Description : null,
                        PublishEndDate = x.PublishEndDate,
                        PublishStartDate = x.PublishStartDate,
                        UserDisplayName = x.User.DisplayName ?? null,
                        SchoolDescription = x.User.UserSchools.Any(y => y.IdUser == x.User.Id) ? x.User.UserSchools.FirstOrDefault(y => y.IdUser == x.User.Id).School.Description : "System",
                        SenderName = !x.IsSetSenderAsSchool ? x.User.DisplayName : null,
                        Subject = !string.IsNullOrEmpty(x.ParentMessageId) ? x.Subject :
                                    !string.IsNullOrEmpty(x.Subject) ? x.Subject : "(No Subject)",
                        Type = x.Type,
                        IsMarkAsPinned = x.IsMarkAsPinned,
                        ProfilePicture = null,
                        ApprovalStatus = x.Status != StatusMessage.OnProgress ? x.Status.ToString() :
                                            x.MessageApprovals.Any(y => !y.IsApproved) ? $"Waiting Approval ({x.MessageApprovals.Where(y => !y.IsApproved).FirstOrDefault().StateNumber})" :
                                                "Message approval not found",
                        IsUnsend = x.IsUnsend,
                        UnsendApprovalStatus = x.StatusUnsend == StatusMessage.Approved ? $"Unsend {StatusMessage.Approved}" :
                                                    x.StatusUnsend == StatusMessage.UnsendRejected ? $"{StatusMessage.UnsendRejected.GetDescription()}" :
                                                        x.MessageApprovals.Any(y => y.IsApproved && !y.IsUnsendApproved) ? $"Waiting Unsend Approval ({x.MessageApprovals.Where(y => y.IsApproved && !y.IsUnsendApproved).OrderBy(y => y.DateIn).FirstOrDefault().StateNumber})" :
                                                            "Message unsend approval not found"
                    })
                    .ToListAsync(CancellationToken)
                : await query
                    .Where(x => (param.Type.HasValue ? x.Type == param.Type : param.MessageFolder == MessageFolder.Sent || param.MessageFolder == MessageFolder.Trash || param.MessageFolder == MessageFolder.Unsend ? true : x.Type != UserMessageType.Information))
                    .OrderByDescending(x => x.DateIn)
                    .Select(x => new GetMessageResult
                    {
                        EndConversation = x.EndConversation,
                        IdSender = x.IsSetSenderAsSchool ? null : x.IdSender,
                        Id = x.Id,
                        Content = WebUtility.HtmlDecode(x.Content),
                        DateIn = x.DateIn,
                        FeedbackType = x.Type == UserMessageType.Feedback ? x.FeedbackType.Description : null,
                        PublishEndDate = x.PublishEndDate,
                        PublishStartDate = x.PublishStartDate,
                        UserDisplayName = x.User.DisplayName ?? null,
                        SchoolDescription = x.User.UserSchools.Any(y => y.IdUser == x.User.Id) ? x.User.UserSchools.FirstOrDefault(y => y.IdUser == x.User.Id).School.Description : "System",
                        SenderName = !x.IsSetSenderAsSchool ? x.User.DisplayName : null,
                        Subject = !string.IsNullOrEmpty(x.ParentMessageId) ? x.Subject :
                                    !string.IsNullOrEmpty(x.Subject) ? x.Subject : "(No Subject)",
                        Type = x.Type,
                        IsMarkAsPinned = x.IsMarkAsPinned,
                        ProfilePicture = null,
                        ApprovalStatus = x.Status != StatusMessage.OnProgress ? x.Status.ToString() :
                                            x.MessageApprovals.Any(y => !y.IsApproved) ? $"Waiting Approval ({x.MessageApprovals.Where(y => !y.IsApproved).FirstOrDefault().StateNumber})" :
                                                "Message approval not found",
                        IsUnsend = x.IsUnsend,
                        UnsendApprovalStatus = x.StatusUnsend == StatusMessage.Approved ? $"Unsend {StatusMessage.Approved}" :
                                                    x.StatusUnsend == StatusMessage.UnsendRejected ? $"{StatusMessage.UnsendRejected.GetDescription()}" :
                                                        x.MessageApprovals.Any(y => y.IsApproved && !y.IsUnsendApproved) ? $"Waiting Unsend Approval ({x.MessageApprovals.Where(y => y.IsApproved && !y.IsUnsendApproved).OrderBy(y => y.DateIn).FirstOrDefault().StateNumber})" :
                                                            "Message unsend approval not found"
                    })
                    .ToListAsync(CancellationToken);

            foreach (var trMessage in trMessages.Where(x => x.SenderName == null).ToList())
            {
                trMessage.SenderName = trMessage.SchoolDescription;
            };

            var messages = trMessages.ToList();

            if (param.MessageFolder == MessageFolder.Inbox)
            {
                if (!string.IsNullOrWhiteSpace(param.Search))
                    messages = messages.Where(x
                        => x.Subject.Contains(param.Search)
                        || x.SenderName.Contains(param.Search)).ToList();
            }

            var listMessages = messages
                .SetPagination(param)
                .Select(x => new GetMessageResult
                {
                    EndConversation = x.EndConversation,
                    IdSender = x.IdSender,
                    Id = x.Id,
                    Content = x.Content,
                    DateIn = x.DateIn,
                    FeedbackType = x.FeedbackType,
                    IsRead = x.IsRead,
                    PublishEndDate = x.PublishEndDate,
                    PublishStartDate = x.PublishStartDate,
                    SenderName = x.SenderName,
                    Subject = x.Subject,
                    Type = x.Type,
                    IsMarkAsPinned = x.IsMarkAsPinned,
                    ProfilePicture = null,
                    Recepients = x.Recepients,
                    ApprovalStatus = x.ApprovalStatus,
                    IsUnsend = x.IsUnsend,
                    UnsendApprovalStatus = x.UnsendApprovalStatus
                })
                .ToList();

            foreach (var message in listMessages)
            {
                message.IsRead = param.IsDraft || param.IsApproval ? false : GetIsReadV2(param, message.Id, trMessageRecepients);
                message.Recepients = GetRecepientsV2(trMessageRecepients.Where(y => y.IdMessage == message.Id).ToList());
            }

            var count = param.CanCountWithoutFetchDb(messages.ToList().Count)
                ? messages.ToList().Count
                : messages.ToList().Count;

            return Request.CreateApiResult2(listMessages as object, param.CreatePaginationProperty(count));
        }
    }
}
