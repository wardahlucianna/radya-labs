using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Auth.Authentications.Jwt.ModelClaims;
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
    public class MCB01X7UserPasswordHandler : FunctionsHttpSingleHandler
    {
        protected readonly IQueryable<MsUser> AuthQuery;
        private readonly IUserDbContext _dbContext;

        private readonly TokenIssuer _tokenIssuer;
        
        public MCB01X7UserPasswordHandler(IUserDbContext userDbContext, TokenIssuer tokenIssuer)
        {
            AuthQuery = userDbContext.Entity<MsUser>()
                .Include(x => x.UserPassword)
                .Include(x => x.UserSchools).ThenInclude(x => x.School)
                .Include(x => x.UserRoles).ThenInclude(x => x.Role).ThenInclude(x => x.RoleGroup);
            
            _tokenIssuer = tokenIssuer;
            _dbContext = userDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<MCB01X7UserPasswordRequest, MCB01X7UserPasswordValidator>();

            body.UserName = body.UserName.Trim();
            body.ImpersonatedUsername = body.ImpersonatedUsername.Trim();

            var query = await AuthQuery.Include(x => x.UserPassword)
                .FirstOrDefaultAsync(x => x.Username == body.UserName, CancellationToken);
            if (query is null)
                throw new NotFoundException("Invalid Login (error code: 07)");
            if (!query.Status)
                throw new BadRequestException("Invalid Login (error code: 02)");
            if (query.UserPassword == null)
                throw new BadRequestException($"Invalid Login (error code: 03)");
            if (query.UserSchools.Count == 0)
                throw new BadRequestException($"Invalid Login (error code: 04)");
            if (query.UserRoles.Count == 0)
                throw new BadRequestException($"Invalid Login (error code: 05)");
            if (!string.IsNullOrEmpty(query.RequestChangePasswordCode) && !query.UsedDate.HasValue)
                throw new BadRequestException("Your password is resetted, please change your password by click link on your email.");

            // impersonated user data
            var impersonatedUserData = _dbContext.Entity<MsUser>()
                                            .Include(x => x.UserSchools)
                                                .ThenInclude(x => x.School)
                                            .Include(x => x.UserRoles)
                                                .ThenInclude(x => x.Role)
                                                .ThenInclude(x => x.RoleGroup)
                                            .Where(x => x.Username == body.ImpersonatedUsername)
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

            if(impersonatedUserData == null)
                throw new NotFoundException("Invalid Login (error code: 07)");

            var queryImpersonate = await AuthQuery.Include(x => x.UserPassword)
                .FirstOrDefaultAsync(x => x.Username == impersonatedUserData.UserName, CancellationToken);

            var user = new MCB01X7UserPasswordResult
            {
                Id = query.Id,
                Email = query.Email,
                IsUserActiveDirectory = query.IsActiveDirectory
            };
            
            if (!query.IsActiveDirectory)
            {
                
                if (query.UserPassword.HashedPassword != (body.Password + query.UserPassword.Salt).ToSHA512())
                    throw new BadRequestException("Invalid Login (error code: 06)");

                var impersonatedUser = new MCB01X7UserPasswordResult
                {
                    Id = impersonatedUserData.IdUser,
                    Email = impersonatedUserData.Email,
                    IsUserActiveDirectory = impersonatedUserData.IsUserActiveDirectory
                };

                user = CreateResult(impersonatedUser, queryImpersonate, body.UserName);
            }

            return Request.CreateApiResult2(user as object);
        }

        protected MCB01X7UserPasswordResult CreateResult(MCB01X7UserPasswordResult baseResult, MsUser msUser, string impersonatorUsername)
        {
            baseResult.Email ??= msUser.Email;
            baseResult.UserName ??= msUser.Username;
            baseResult.DisplayName ??= msUser.DisplayName;
            baseResult.Token = _tokenIssuer.IssueTokenForUser(
                new ClaimsToken
                {
                    UserId = msUser.Id,
                    UserName = msUser.Username,
                    Roles = msUser.UserRoles.OrderByDescending(role => role.IsDefault).Select(role => new ClaimsRole
                    {
                        Id = role.Role.Id,
                        Code = role.Role.Code,
                        Name = role.Role.Description
                    }),
                    Tenants = msUser.UserSchools.Select(school => new ClaimsTenant
                    {
                        Id = school.IdSchool,
                        Name = school.School.Name
                    })
                },
                8);
            baseResult.Roles = msUser.UserRoles.OrderByDescending(role => role.IsDefault).Select(role => new Data.Model.User.FnAuth.ImpersonateLogin.UserRoleResult
            {
                Id = role.Role.Id,
                Name = role.Role.Description,
                CanOpenTeacherTracking = role.Role.IsCanOpenTeacherTracking,
                RoleGroup = new NameValueVm
                {
                    Id = role.Role.RoleGroup.Id,
                    Name = role.Role.RoleGroup.Description
                }
            });
            baseResult.Schools = msUser.UserSchools.Select(school => new SchoolResult
            {
                Name = school.School.Name,
                Id = school.School.Id,
                Logo = ""
            });
            baseResult.ImpersonatorIdUser = AuthQuery.FirstOrDefault(x => x.Username == impersonatorUsername).Id;


            return baseResult;
        }
    }
}
