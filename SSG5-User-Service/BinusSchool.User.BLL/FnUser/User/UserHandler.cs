using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.User.FnCommunication;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Data.Model.User.FnCommunication.Message;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.User.Validator;
using BinusSchool.User.FnUser.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.User.FnUser.User
{
    public class UserHandler : FunctionsHttpCrudHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly INotificationManager _notificationManager;
        private readonly IMessage _messageService;
        private readonly IEventSchool _eventSchool;

        public UserHandler(
            IUserDbContext dbContext,
            IConfiguration configuration,
            INotificationManager notificationManager, IMessage messageService, IEventSchool eventSchool
            )
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _notificationManager = notificationManager;
            _messageService = messageService;
            _eventSchool = eventSchool;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsUser>()
                .Include(x => x.UserPassword)
                .Include(x => x.UserRoles).ThenInclude(e=>e.Role)
                .Include(x => x.UserSchools)
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                data.IsActive = false;
                _dbContext.Entity<MsUser>().Update(data);

                if (data.UserPassword != null)
                {
                    data.UserPassword.IsActive = false;
                    _dbContext.Entity<MsUserPassword>().Update(data.UserPassword);
                }

                foreach (var role in data.UserRoles)
                {
                    role.IsActive = false;
                    _dbContext.Entity<MsUserRole>().Update(role);
                }

                foreach (var school in data.UserSchools)
                {
                    school.IsActive = false;
                    _dbContext.Entity<MsUserSchool>().Update(school);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            var idSchool = datas.SelectMany(e => e.UserRoles.Select(e => e.Role.IdSchool)).FirstOrDefault();
            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = idSchool
            });

            var apiqueueEvent = await _eventSchool.QueueEvent(new QueueEventRequest
            {
                IdSchool = idSchool
            });
            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var result = await _dbContext.Entity<MsUser>()
                                         .Include(x => x.UserRoles)
                                            .ThenInclude(x => x.Role)
                                                .ThenInclude(x => x.RoleSettings)
                                        .Include(x=>x.UserRoles)
                                            .ThenInclude(x=>x.Role)
                                                .ThenInclude(x=>x.RoleGroup)
                                         .Select(x => new GetUserDetailResult
                                         {
                                             Id = x.Id,
                                             IsActiveDirectory = x.IsActiveDirectory,
                                             Email = x.Email,
                                             DisplayName = x.DisplayName,
                                             Username = x.Username,
                                             Roles = x.UserRoles.Where(y => y.IsActive).Select(y => new UserRoleResult
                                             {
                                                 Id = y.Role.Id,
                                                 Code = y.Role.Code,
                                                 Description = y.Role.Description,
                                                 IsDefaultUsername = y.IsDefault,
                                                 Username = y.Username,
                                                 RoleGroup = new CodeWithIdVm
                                                 {
                                                     Id = y.Role.RoleGroup.Id,
                                                     Code = y.Role.RoleGroup.Code,
                                                     Description = y.Role.RoleGroup.Description
                                                 },
                                                 UsernameFormat = y.Role.RoleSettings.Any(z => z.IsArrangeUsernameFormat) ?
                                                                   y.Role.RoleSettings.Where(z => z.IsArrangeUsernameFormat)
                                                                                      .Select(z => z.UsernameFormat)
                                                                                      .First() : null
                                             })
                                         })
                                         .SingleOrDefaultAsync(x => x.Id == id);
            if (result is null)
                throw new NotFoundException("User is not found");

            return Request.CreateApiResult2(result as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetUserRequest>(nameof(GetUserRequest.IdSchool));

            var columns = new[] { "displayName", "email", "username", "isActiveDirectory" };

            var predicate = PredicateBuilder.True<MsUser>();
            if (param.IdSchool?.Any() ?? false)
                predicate = predicate.And(x => x.UserSchools.Any(y => param.IdSchool.Contains(y.IdSchool)));
            if (!string.IsNullOrEmpty(param.IdRole))
                predicate = predicate.And(x => x.UserRoles.Any(y => y.IdRole == param.IdRole));
            if (!string.IsNullOrEmpty(param.RoleGroupCode))
                predicate = predicate.And(x => x.UserRoles.Any(y => y.Role.RoleGroup.Code == param.RoleGroupCode));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.DisplayName, param.SearchPattern())
                    || EF.Functions.Like(x.Email, param.SearchPattern())
                    || EF.Functions.Like(x.Username, param.SearchPattern()));

            var query = _dbContext.Entity<MsUser>()
                .Include(x => x.UserRoles).ThenInclude(x => x.Role).ThenInclude(x => x.RoleGroup)
                .SearchByIds(param)
                .Where(predicate)
                .SearchByDynamic(param)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.DisplayName))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new ListUserResult
                    {
                        Id = x.Id,
                        Code = x.Id,
                        Description = x.DisplayName,
                        Email = x.Email,
                        Username = x.Username,
                        IsActiveDirectory = x.IsActiveDirectory,
                        Role = string.Join(", ", x.UserRoles.Where(y => y.IsActive).Select(p => p.Role.Description)),
                        IsActive = x.Status,
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddUserRequest, AddUserValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var idRoles = body.Roles.Select(x => x.IdRole).ToList();
            if (await _dbContext.Entity<LtRole>()
                                .CountAsync(x => x.IdSchool == body.IdSchool
                                                 && idRoles.Contains(x.Id)) != idRoles.Count())
                throw new NotFoundException("Some role is not found");

            var username = body.Roles.First(x => x.IsDefaultUsername).Username;
            if (await _dbContext.Entity<MsUser>().AnyAsync(x => x.Username.ToLower() == username.ToLower()))
                throw new BadRequestException("Username is taken");
            // if (await _dbContext.Entity<MsUser>().AnyAsync(x => x.Email.ToLower() == body.Email.ToLower()))
            //     throw new BadRequestException("Email is taken");

            // var userId = Guid.NewGuid().ToString();
            var userId = body.Roles.First(x => x.IsDefaultUsername).Username;
            var changePasswordCode = Guid.NewGuid().ToString();
            _dbContext.Entity<MsUser>().Add(new MsUser
            {
                Id = userId,
                Username = body.Roles.First(x => x.IsDefaultUsername).Username,
                DisplayName = body.DisplayName,
                Email = body.Email,
                IsActiveDirectory = body.IsActiveDirectory,
                RequestChangePasswordCode = changePasswordCode,
                Status = true
            });

            _dbContext.Entity<MsUserSchool>().Add(new MsUserSchool
            {
                Id = Guid.NewGuid().ToString(),
                IdUser = userId,
                IdSchool = body.IdSchool
            });

            _dbContext.Entity<MsUserRole>().AddRange(body.Roles.Select(x => new MsUserRole
            {
                Id = Guid.NewGuid().ToString(),
                IdUser = userId,
                IdRole = x.IdRole,
                IsDefault = x.IsDefaultUsername,
                Username = x.Username
            }));

            // add random password and send email
            var password = Generator.GenerateRandomPassword(8);
            var salt = Generator.GenerateSalt();
            _dbContext.Entity<MsUserPassword>().Add(new MsUserPassword
            {
                Id = userId,
                Salt = salt,
                HashedPassword = (password + salt).ToSHA512()
            });

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = body.IdSchool
            });

            var apiqueueEvent = await _eventSchool.QueueEvent(new QueueEventRequest
            {
                IdSchool = body.IdSchool
            });

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "USR1")
                {
                    IdRecipients = new[] { userId },
                    KeyValues = new Dictionary<string, object>
                    {
                        { "fullName", body.DisplayName },
                        { "userEmail", body.Email },
                        { "userName", username },
                        { "idRoles", body.Roles.Select(x => x.IdRole) },
                        { "idUserAdmin", AuthInfo.UserId },
                        { "passwordCode", changePasswordCode },
                    }
                });
                collector.Add(message);
            }

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateUserRequest, UpdateUserValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var idRoles = body.Roles.Select(x => x.IdRole).ToList();
            if (await _dbContext.Entity<LtRole>()
                                .CountAsync(x => x.IdSchool == body.IdSchool
                                                 && idRoles.Contains(x.Id)) != idRoles.Count())
                throw new NotFoundException("Some role is not found");

            var username = body.Roles.First(x => x.IsDefaultUsername).Username;
            if (await _dbContext.Entity<MsUser>().AnyAsync(x => x.Id != body.Id && x.Username.ToLower() == username.ToLower()))
                throw new BadRequestException("Username is taken");
            // if (await _dbContext.Entity<MsUser>().AnyAsync(x => x.Id != body.Id && x.Email.ToLower() == body.Email.ToLower()))
            //     throw new BadRequestException("Email is taken");

            var data = await _dbContext.Entity<MsUser>()
                                       .Include(x => x.UserRoles)
                                       .Include(x => x.UserSchools)
                                       .Where(x => x.Id == body.Id
                                                   && x.UserSchools.Any(y => y.IdSchool == body.IdSchool))
                                       .SingleOrDefaultAsync();
            if (data is null)
                throw new NotFoundException("User is not found");

            data.Username = username;
            data.DisplayName = body.DisplayName;
            data.Email = body.Email;
            data.IsActiveDirectory = body.IsActiveDirectory;
            _dbContext.Entity<MsUser>().Update(data);

            foreach (var role in data.UserRoles.Where(x => x.IsActive))
            {
                if (idRoles.Contains(role.IdRole))
                {
                    var value = body.Roles.First(x => x.IdRole == role.IdRole);
                    role.IsDefault = value.IsDefaultUsername;
                    role.Username = value.Username;
                }
                else
                    role.IsActive = false;

                _dbContext.Entity<MsUserRole>().Update(role);
            }

            _dbContext.Entity<MsUserRole>().AddRange(body.Roles.Where(x => !data.UserRoles.Where(x => x.IsActive).Select(y => y.IdRole).Contains(x.IdRole)).Select(x => new MsUserRole
            {
                Id = Guid.NewGuid().ToString(),
                IdUser = body.Id,
                IdRole = x.IdRole,
                IsDefault = x.IsDefaultUsername,
                Username = x.Username
            }));

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            var apiqueueMessage = await _messageService.QueueMessages(new QueueMessagesRequest
            {
                IdSchool = body.IdSchool
            });

            var apiqueueEvent = await _eventSchool.QueueEvent(new QueueEventRequest
            {
                IdSchool = body.IdSchool
            });
            return Request.CreateApiResult2();
        }
    }
}
