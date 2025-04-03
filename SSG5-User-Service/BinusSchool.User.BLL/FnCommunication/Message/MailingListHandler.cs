using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnCommunication.Message.Validator;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;

namespace BinusSchool.User.FnCommunication.Message
{
    public class MailingListHandler : FunctionsHttpCrudHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMessage _messageService;
        public MailingListHandler(IUserDbContext userDbContext, IMessage messageService)
        {
            _dbContext = userDbContext;
            _messageService = messageService;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var data = await _dbContext.Entity<MsGroupMailingList>()
                .Where(x => ids.Any(y => y == x.Id))
                .FirstOrDefaultAsync(CancellationToken);

            if (data == null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["GroupMailingList"], "Id", ids.FirstOrDefault()));

            if (data != null)
            {
                data.IsActive = false;
                _dbContext.Entity<MsGroupMailingList>().Update(data);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = data.IdSchool
            });

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsGroupMailingList>()
                .Include(x => x.User)
                .Select(x => new GetGroupMailingListDetailsResult
                {
                    Id = x.Id,
                    GroupName = x.GroupName,
                    GroupDescripction = x.Description,
                    Description = x.Description,
                    IdUser = x.IdUser,
                    OwnerGroup = x.User.DisplayName,
                    UserName = x.User.Username,
                    CreateBy = x.UserIn,
                    CreateDate = x.DateIn.GetValueOrDefault(),
                    GroupMembers = new List<GroupMember>()
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data != null)
            {
                var dataMember = await _dbContext.Entity<MsGroupMailingListMember>()
                    .Include(x => x.User)
                    .ThenInclude(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                    .ThenInclude(x => x.RoleGroup)
                    .Where(x => x.IdGroupMailingList == id)
                    .ToListAsync();

                data.GroupMembers = new List<GroupMember>();
                foreach (var item in dataMember)
                {
                    var GroupMember = new GroupMember();
                    GroupMember.Id = item.Id;
                    GroupMember.IdUser = item.IdUser;
                    GroupMember.Name = item.User.DisplayName;
                    GroupMember.UserName = item.User.Username;
                    GroupMember.Role = item.User.UserRoles.First().Role.RoleGroup.Description;
                    GroupMember.CreateMessage = item.IsCreateMessage;

                    data.GroupMembers.Add(GroupMember);
                }
            }

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetGroupMailingListRequest>(nameof(GetGroupMailingListRequest.IdUser));
            var columns = new[] { "name", "joindate", "grouprole" };

            var predicate = PredicateBuilder.Create<MsGroupMailingList>(x => x.IsActive);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.GroupName, $"%{param.Search}%")
                    || EF.Functions.Like(x.Description, $"%{param.Search}%"));

            if (!string.IsNullOrWhiteSpace(param.GroupName))
                predicate = predicate.And(x => x.GroupName.Contains(param.GroupName));

            var query = _dbContext.Entity<MsGroupMailingList>()
            .Include(x => x.GroupMailingListMembers)
            .Where(predicate)
            .Where(x => x.IdUser == param.IdUser || x.GroupMailingListMembers.Any(y => y.IdUser == param.IdUser))
            .OrderByDynamic(param);

            switch (param.OrderBy?.ToLower())
            {
                case "groupname":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.GroupName)
                        : query.OrderBy(x => x.GroupName);
                    break;
                case "grouprole":
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdUser == param.IdUser ? "Owner" : "Member")
                        : query.OrderBy(x => x.IdUser == param.IdUser ? "Owner" : "Member");
                    break;
                default:
                    query = param.OrderType == OrderType.Desc
                        ? query.OrderByDescending(x => x.IdUser == param.IdUser ? x.DateIn.Value : x.GroupMailingListMembers.First().DateIn.Value)
                        : query.OrderBy(x => x.IdUser == param.IdUser ? x.DateIn.Value : x.GroupMailingListMembers.First().DateIn.Value);
                    break;
            };

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            else
            {
                if (param.IsCreateMessage != null)
                {
                    items = await query
                        .SetPagination(param)
                        .Select(x => new GetGroupMailingListResult
                        {
                            Id = x.Id,
                            Description = x.Description,
                            GroupName = x.GroupName,
                            JoinDate = x.IdUser == param.IdUser ? x.DateIn.GetValueOrDefault() : x.GroupMailingListMembers.First().DateIn.GetValueOrDefault(),
                            GroupRole = x.IdUser == param.IdUser ? "Owner" : "Member",
                            CreateMessage = x.IdUser == param.IdUser ? true : x.GroupMailingListMembers.First().IsCreateMessage,
                        })
                        .Where(x => x.CreateMessage == param.IsCreateMessage)
                        .ToListAsync(CancellationToken);
                }
                else
                {
                    items = await query
                        .SetPagination(param)
                        .Select(x => new GetGroupMailingListResult
                        {
                            Id = x.Id,
                            Description = x.Description,
                            GroupName = x.GroupName,
                            JoinDate = x.IdUser == param.IdUser ? x.DateIn.GetValueOrDefault() : x.GroupMailingListMembers.First().DateIn.GetValueOrDefault(),
                            GroupRole = x.IdUser == param.IdUser ? "Owner" : "Member",
                            CreateMessage = x.IdUser == param.IdUser ? true : x.GroupMailingListMembers.First().IsCreateMessage,
                        })
                        .ToListAsync(CancellationToken);
                }
            }


            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddGroupMailingListRequest, AddGroupMailingListValidator>();

            var cekDataMailingList = _dbContext.Entity<MsGroupMailingList>().Where(x => x.GroupName.ToLower() == body.GroupName.ToLower() && x.IdSchool == body.IdSchool).FirstOrDefault();
            if (cekDataMailingList != null)
                throw new BadRequestException(string.Format(Localizer["ExAlreadyExist"], Localizer["GroupMailingList"], "Group Name", body.GroupName));

            var getUserSchool = _dbContext.Entity<MsUserSchool>()
                .Include(x => x.User)
                .Where(x => x.IdUser == body.IdUser).FirstOrDefault();

            if (getUserSchool != null)
            {
                var idMemberGroups = body.MemberLists.Select(x => x.IdUser).ToList();
                if (idMemberGroups.Any())
                {
                    var validateMemberSchools = _dbContext.Entity<MsUserSchool>()
                        .Include(x => x.User)
                        .Where(x => idMemberGroups.Any(m => m == x.IdUser) && x.IdSchool != getUserSchool.IdSchool).Select(x => x.User.DisplayName).ToList();

                    if (validateMemberSchools.Any())
                    {
                        throw new BadRequestException(string.Format(Localizer["ExSchoolData"], string.Join(",", validateMemberSchools), getUserSchool.User.DisplayName));
                    }
                }
            }

            var groupMailingList = new MsGroupMailingList
            {
                Id = Guid.NewGuid().ToString(),
                GroupName = body.GroupName,
                Description = body.Description,
                IdUser = body.IdUser,
                IdSchool = body.IdSchool
            };

            _dbContext.Entity<MsGroupMailingList>().Add(groupMailingList);

            var memberLists = new List<MsGroupMailingListMember>();
            foreach (var item in body.MemberLists)
            {
                var memberList = new MsGroupMailingListMember()
                {
                    Id = Guid.NewGuid().ToString(),
                    IdGroupMailingList = groupMailingList.Id,
                    IdUser = item.IdUser,
                    IsCreateMessage = item.CreateMessage
                };

                memberLists.Add(memberList);
            }
            _dbContext.Entity<MsGroupMailingListMember>().AddRange(memberLists);

            await _dbContext.SaveChangesAsync(CancellationToken);

            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = body.IdSchool
            });

            #region Send Email
            var dataGroupMailingList = _dbContext.Entity<MsGroupMailingList>()
                    .Include(x => x.GroupMailingListMembers)
                    .Include(x => x.User)
                    .Where(x => x.Id == groupMailingList.Id)
                    .FirstOrDefault();

            if (dataGroupMailingList != null)
            {
                var DetailGroupMailingList = new GetGroupMailingListDetailsResult
                {
                    Id = dataGroupMailingList.Id,
                    GroupName = dataGroupMailingList.GroupName,
                    GroupDescripction = dataGroupMailingList.Description,
                    IdUser = dataGroupMailingList.IdUser,
                    GroupMembers = new List<GroupMember> { new GroupMember {
                        Name = dataGroupMailingList.User.DisplayName
                    }},
                    IdListMember = dataGroupMailingList.GroupMailingListMembers.Select(e => e.IdUser).ToList(),
                };

                if (KeyValues.ContainsKey("GetInvitationBookingSettingEmail"))
                {
                    KeyValues.Remove("GetInvitationBookingSettingEmail");
                }
                KeyValues.Add("GetDetailGroupMailingList", DetailGroupMailingList);
                var Notification = EHN1Notification(KeyValues, AuthInfo);
            }

            #endregion

            #region Send Notification
            var idMembers = dataGroupMailingList.GroupMailingListMembers.Where(ml => ml.IsCreateMessage).Select(x => x.IdUser).ToList();
            if (idMembers.Any())
            {
                var userMembers = _dbContext.Entity<MsUser>().Where(x => idMembers.Any(y => y == x.Id)).ToList();
                if (userMembers.Any())
                {
                    var DetailGroupMailingListInApp = new GetGroupMailingListDetailsResult
                    {
                        Id = groupMailingList.Id,
                        GroupName = groupMailingList.GroupName,
                        IdUser = groupMailingList.IdUser,
                        OwnerGroup = getUserSchool.User.DisplayName,
                        GroupMembers = userMembers.Select(s => new GroupMember
                        {
                            Name = s.DisplayName
                        }).ToList(),
                        IdListMember = idMembers
                    };

                    if (KeyValues.ContainsKey("GetDetailGroupMailingList"))
                    {
                        KeyValues.Remove("GetDetailGroupMailingList");
                    }
                    KeyValues.Add("GetDetailGroupMailingList", DetailGroupMailingListInApp);
                    var NotificationInApp = EHN2Notification(KeyValues, AuthInfo);
                }

            }

            #endregion

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateGroupMailingListRequest, UpdateGroupMailingListValidator>();

            var data = await _dbContext.Entity<MsGroupMailingList>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["GroupMailingList"], "Id", body.Id));

            data.GroupName = body.GroupName;
            data.Description = body.Description;

            _dbContext.Entity<MsGroupMailingList>().Update(data);

            var dataMember = _dbContext.Entity<MsGroupMailingListMember>().Where(x => x.IdGroupMailingList == body.Id).ToList();
            var dataIdUserMember = dataMember.Select(e => e.IdUser).ToList();
            var newMember = body.MemberLists.Where(e => !dataIdUserMember.Contains(e.IdUser)).ToList();
            var NewMamberCreateMessage = newMember.Where(e => e.CreateMessage == true).Select(e=>e.IdUser).ToList();
            var dataMemberOld = body.MemberLists.Where(e => dataIdUserMember.Contains(e.IdUser)).ToList();

            foreach(var item in dataMemberOld)
            {
                if(item.CreateMessage == true)
                {
                    var changeStatusCreateMessage = dataMember.Where(e => e.IdUser == item.IdUser && !e.IsCreateMessage).FirstOrDefault();

                    if (changeStatusCreateMessage != null)
                    {
                        NewMamberCreateMessage.Add(changeStatusCreateMessage.IdUser);
                        changeStatusCreateMessage.IsCreateMessage= true;
                        _dbContext.Entity<MsGroupMailingListMember>().Update(changeStatusCreateMessage);
                    }
                }
            }

            var memberLists = new List<MsGroupMailingListMember>();
            foreach (var item in newMember)
            {
                var memberList = new MsGroupMailingListMember();
                memberList.IdGroupMailingList = body.Id;
                memberList.Id = Guid.NewGuid().ToString();
                memberList.IdUser = item.IdUser;
                memberList.IsCreateMessage = item.CreateMessage;

                memberLists.Add(memberList);
            }
            _dbContext.Entity<MsGroupMailingListMember>().AddRange(memberLists);

            await _dbContext.SaveChangesAsync(CancellationToken);

            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = data.IdSchool
            });

            #region Send Email
            var dataGroupMailingList = _dbContext.Entity<MsGroupMailingList>()
                    .Include(x => x.GroupMailingListMembers)
                    .Include(x => x.User)
                    .Where(x => x.Id == body.Id)
                    .FirstOrDefault();

            if (dataGroupMailingList != null && newMember.Any())
            {
                var DetailGroupMailingList = new GetGroupMailingListDetailsResult
                {
                    Id = dataGroupMailingList.Id,
                    GroupName = dataGroupMailingList.GroupName,
                    GroupDescripction = dataGroupMailingList.Description,
                    IdUser = dataGroupMailingList.IdUser,
                    GroupMembers = new List<GroupMember> { new GroupMember {
                        Name = dataGroupMailingList.User.DisplayName
                    }},
                    IdListMember = newMember.Select(e => e.IdUser).ToList(),
                };

                if (KeyValues.ContainsKey("GetInvitationBookingSettingEmail"))
                {
                    KeyValues.Remove("GetInvitationBookingSettingEmail");
                }
                KeyValues.Add("GetDetailGroupMailingList", DetailGroupMailingList);
                var Notification = EHN1Notification(KeyValues, AuthInfo);
            }

            #endregion

            #region Send Notification
            if (NewMamberCreateMessage.Any())
            {
                var userMembers = _dbContext.Entity<MsUser>().Where(x => NewMamberCreateMessage.Any(y => y == x.Id)).ToList();
                if (userMembers.Any())
                {
                    var DetailGroupMailingListInApp = new GetGroupMailingListDetailsResult
                    {
                        Id = dataGroupMailingList.Id,
                        GroupName = dataGroupMailingList.GroupName,
                        IdUser = dataGroupMailingList.IdUser,
                        OwnerGroup = dataGroupMailingList.User.DisplayName,
                        GroupMembers = default,
                        IdListMember = NewMamberCreateMessage
                    };

                    if (KeyValues.ContainsKey("GetDetailGroupMailingList"))
                    {
                        KeyValues.Remove("GetDetailGroupMailingList");
                    }
                    KeyValues.Add("GetDetailGroupMailingList", DetailGroupMailingListInApp);
                    var NotificationInApp = EHN2Notification(KeyValues, AuthInfo);
                }

            }

            #endregion

            return Request.CreateApiResult2();
        }

        public static string EHN1Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDetailGroupMailingList").Value;
            var DetailMailingList = JsonConvert.DeserializeObject<GetGroupMailingListDetailsResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "EHN1")
                {
                    IdRecipients = new List<string>()
                    {
                        DetailMailingList.IdListMember.First(),
                    },
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
        public static string EHN2Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDetailGroupMailingList").Value;
            var DetailMailingList = JsonConvert.DeserializeObject<GetGroupMailingListDetailsResult>(JsonConvert.SerializeObject(Object));

            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "EHN2")
                {
                    IdRecipients = DetailMailingList.IdListMember.Select(x => x), // new string[] { data },
                    KeyValues = KeyValues
                });

                collector.Add(message);
            }
            return "";
        }
    }
}
