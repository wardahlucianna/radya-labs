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
using BinusSchool.Data.Model.User.FnAuth.ImpersonateLogin;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnAuth.ImpersonateLogin.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnAuth.ImpersonateLogin
{
    public class ImpersonateLoginHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public ImpersonateLoginHandler(IUserDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<ImpersonateLoginRequest, ImpersonateLoginValidator>();

            param.ImpersonatorIdUser = param.ImpersonatorIdUser.Trim();
            param.ImpersonatedUsername = param.ImpersonatedUsername.Trim();

            // check if user authenticated to use impersonate login
            var accessImpersonateId = await _dbContext.Entity<MsAccessImpersonateUser>()
                                            .Where(x => x.IdUser == param.ImpersonatorIdUser &&
                                                        x.CanImpersonateLogin == true)
                                            .Select(x => x.Id)
                                            .FirstOrDefaultAsync(CancellationToken);

            if (accessImpersonateId == null)
                throw new BadRequestException("Unauthorized to use impersonate login");

            // impersonated user data
            var impersonatedUserData = _dbContext.Entity<MsUser>()
                                            .Include(x => x.UserSchools)
                                                .ThenInclude(x => x.School)
                                            .Include(x => x.UserRoles)
                                                .ThenInclude(x => x.Role)
                                                .ThenInclude(x => x.RoleGroup)
                                            .Where(x => x.Username == param.ImpersonatedUsername)
                                            .Select(x => new ImpersonateLoginResult
                                            {
                                                IdUser = x.Id,
                                                UserName = x.Username,
                                                DisplayName = x.DisplayName,
                                                Email = x.Email,
                                                IsUserActiveDirectory = x.IsActiveDirectory,
                                                Roles = x.UserRoles.OrderByDescending(role => role.IsDefault).Select(role => new ImpersonateLoginResult_UserRoleResult
                                                {
                                                    Id = role.Role.Id,
                                                    Name = role.Role.Description,
                                                    CanOpenTeacherTracking = role.Role.IsCanOpenTeacherTracking,
                                                    RoleGroup = new NameValueWithCodeVm
                                                    {
                                                        Id = role.Role.RoleGroup.Id,
                                                        Name = role.Role.RoleGroup.Description,
                                                        Code = role.Role.RoleGroup.Code
                                                    }
                                                }),
                                                Schools = x.UserSchools.Select(school => new ImpersonateLoginResult_SchoolResult
                                                {
                                                    Name = school.School.Name,
                                                    Id = school.School.Id,
                                                    Logo = ""
                                                })
                                            })
                                            .FirstOrDefault();

            if (impersonatedUserData == null)
                throw new BadRequestException($"Failed to impersonate this username ({param.ImpersonatedUsername})");

            // insert log to TrAccessImpersonateUserLog
            var log = new TrAccessImpersonateUserLog
            {
                Id = Guid.NewGuid().ToString(),
                UserIn = param.ImpersonatorIdUser,
                IdAccessImpersonateUser = accessImpersonateId,
                ImpersonatedIdUser = impersonatedUserData.IdUser,
                LoginTime = _dateTime.ServerTime,
                IpAddress = param.LoggedInIpAddress
            };

            _dbContext.Entity<TrAccessImpersonateUserLog>().Add(log);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2(impersonatedUserData as object);
        }
    }
}
