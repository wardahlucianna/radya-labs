using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.LoginAs.Validator;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.User.FnUser.LoginAs.ImpersonateUser
{
    public class ImpersonateUserLoginAsHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly CheckUserNamePasswordImpersonateUserLoginAsHandler _checkUserNamePasswordImpersonateUserLoginAsHandler;

        public ImpersonateUserLoginAsHandler(
            IUserDbContext dbContext, 
            IMachineDateTime dateTime,
            CheckUserNamePasswordImpersonateUserLoginAsHandler checkUserNamePasswordImpersonateUserLoginAsHandler
            )
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _checkUserNamePasswordImpersonateUserLoginAsHandler = checkUserNamePasswordImpersonateUserLoginAsHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<ImpersonateUserLoginAsRequest, ImpersonateUserLoginAsValidator>();

            param.IdCurrentUser = param.IdCurrentUser.Trim();
            param.ImpresonatingUsername = param.ImpresonatingUsername.Trim();

            // check if user authenticated to use impersonate login
            var getRoleImpersonator = await _dbContext.Entity<MsUserRole>()
                    .Where(x => x.IdUser == param.IdCurrentUser)
                    .Select(x => x.IdRole)
                    .ToListAsync(CancellationToken);

            var getRoleIImpersonated = await _dbContext.Entity<MsUserRole>()
                    .Where(x => x.Username == param.ImpresonatingUsername)
                    .Select(x => x.IdRole)
                    .ToListAsync(CancellationToken);

            var getAllRoleLoginAs = await _dbContext.Entity<TrRoleLoginAs>()
                    .Where(x => x.Role.IdSchool == param.IdSchool &&
                                x.AuthorizedRole.IdSchool == param.IdSchool &&
                                getRoleImpersonator.Contains(x.IdRole))
                    .Select(x => x.IdAuthorizedRole)
                    .Distinct()
                    .ToListAsync(CancellationToken);

            //var impersonatedLogin = JsonConvert.SerializeObject(getAllRoleLoginAs, Formatting.Indented);

            var IsImpersonatorHaveAccess = getAllRoleLoginAs
                    .Any(y => getRoleIImpersonated.Contains(y));

            if (!IsImpersonatorHaveAccess)
                throw new BadRequestException("Unauthorized to use impersonate login");

            // impersonated user data
            var impersonatedUserData = _dbContext.Entity<MsUser>()
                    .Include(x => x.UserSchools)
                        .ThenInclude(x => x.School)
                    .Include(x => x.UserRoles)
                        .ThenInclude(x => x.Role)
                        .ThenInclude(x => x.RoleGroup)
                    .Where(x => x.Username == param.ImpresonatingUsername)
                    .Select(x => new ImpersonateUserLoginAsResult
                    {
                        IdUser = x.Id,
                        UserName = x.Username,
                        DisplayName = x.DisplayName,
                        Email = x.Email,
                        IsUserActiveDirectory = x.IsActiveDirectory,
                        Roles = x.UserRoles.OrderByDescending(role => role.IsDefault).Select(role => new ImpersonateUserLoginAsResult_UserRoleResult
                        {
                            Id = role.Role.Id,
                            Code = role.Role.Code,
                            Description = role.Role.Description,
                            IsDefault = role.IsDefault,
                            CanOpenTeacherTracking = role.Role.IsCanOpenTeacherTracking,
                            RoleGroup = new NameValueWithCodeVm
                            {
                                Id = role.Role.RoleGroup.Id,
                                Name = role.Role.RoleGroup.Description,
                                Code = role.Role.RoleGroup.Code
                            }
                        }),
                        Schools = x.UserSchools.Select(school => new ImpersonateUserLoginAsResult_SchoolResult
                        {
                            Name = school.School.Name,
                            Id = school.School.Id,
                            Logo = ""
                        })
                    })
                    .FirstOrDefault();

            if (impersonatedUserData == null)
                throw new BadRequestException($"Failed to impersonate this username ({param.ImpresonatingUsername})");

            var checkUserNamePassword = await _checkUserNamePasswordImpersonateUserLoginAsHandler.CheckUserNamePasswordImpersonateUserLoginAs(new CheckUserNamePasswordImpersonateUserLoginAsRequest
            {
                ImpersonatedUsername = param.ImpresonatingUsername
            });

            impersonatedUserData.Token = checkUserNamePassword.Token;
            impersonatedUserData.IsBlock = checkUserNamePassword.IsBlock;
            impersonatedUserData.BlockingMessage = checkUserNamePassword.BlockingMessage;

            // insert log to TrLoginAs 
            var log = new TrLoginAs
            {
                Id = Guid.NewGuid().ToString(),
                IdCurrentUser = param.IdCurrentUser,
                IdImpresonatingUser = impersonatedUserData.IdUser,
                IpAddress = param.LoggedInIpAddress,
                LoginTime = _dateTime.ServerTime,
            };

            _dbContext.Entity<TrLoginAs>().Add(log);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2(impersonatedUserData as object);
        }
    }
}
