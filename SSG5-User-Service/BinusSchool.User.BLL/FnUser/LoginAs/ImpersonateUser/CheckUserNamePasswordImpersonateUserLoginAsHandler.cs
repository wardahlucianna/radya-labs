using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Auth.Authentications.Jwt.ModelClaims;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnUser.LoginAs.Validator;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace BinusSchool.User.FnUser.LoginAs.ImpersonateUser
{
    public class CheckUserNamePasswordImpersonateUserLoginAsHandler : FunctionsHttpSingleHandler
    {
        protected readonly IQueryable<MsUser> AuthQuery;
        private IDictionary<string, object> _DataMessageBlocking;
        private readonly IUserDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;
        private readonly TokenIssuer _tokenIssuer;

        public CheckUserNamePasswordImpersonateUserLoginAsHandler(IUserDbContext userDbContext, TokenIssuer tokenIssuer, [FromServices] IFeatureManagerSnapshot featureManager)
        {
            AuthQuery = userDbContext.Entity<MsUser>()
                .Include(x => x.UserPassword)
                .Include(x => x.UserSchools).ThenInclude(x => x.School)
                .Include(x => x.UserRoles).ThenInclude(x => x.Role).ThenInclude(x => x.RoleGroup);

            _tokenIssuer = tokenIssuer;
            _dbContext = userDbContext;
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CheckUserNamePasswordImpersonateUserLoginAsRequest, CheckUserNamePasswordImpersonateUserLoginAsValidator>();

            var result = await CheckUserNamePasswordImpersonateUserLoginAs(new CheckUserNamePasswordImpersonateUserLoginAsRequest
            {
                //UserName = body.UserName,
                //DisplayName = body.DisplayName,
                //Password = body.Password,
                ImpersonatedUsername = body.ImpersonatedUsername
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<CheckUserNamePasswordImpersonateUserLoginAsResult> CheckUserNamePasswordImpersonateUserLoginAs(CheckUserNamePasswordImpersonateUserLoginAsRequest body)
        {
            //body.UserName = body.UserName.Trim();
            body.ImpersonatedUsername = body.ImpersonatedUsername.Trim();

            var query = await AuthQuery.Include(x => x.UserPassword)
                .FirstOrDefaultAsync(x => x.Username == body.ImpersonatedUsername, CancellationToken);
            if (query is null)
                throw new NotFoundException("Invalid Login (error code: 07)");
            if (!query.Status)
                throw new BadRequestException("Invalid Login (error code: 02)");
            //if (query.UserPassword == null)
            //    throw new BadRequestException($"Password for user {query.Username} has not been set");
            if (query.UserSchools.Count == 0)
                throw new BadRequestException($"Invalid Login (error code: 04)");
            if (query.UserRoles.Count == 0)
                throw new BadRequestException($"Invalid Login (error code: 05)");
            //if (!string.IsNullOrEmpty(query.RequestChangePasswordCode) && !query.UsedDate.HasValue)
            //    throw new BadRequestException("Your password is resetted, please change your password by click link on your email.");

            // impersonated user data
            var impersonatedUserData = _dbContext.Entity<MsUser>()
                                            //.Include(x => x.UserSchools)
                                            //    .ThenInclude(x => x.School)
                                            //.Include(x => x.UserRoles)
                                            //    .ThenInclude(x => x.Role)
                                            //    .ThenInclude(x => x.RoleGroup)
                                            .Where(x => x.Username == body.ImpersonatedUsername)
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
                throw new NotFoundException("Invalid Login (error code: 07)");

            var user = new CheckUserNamePasswordImpersonateUserLoginAsResult
            {
                Id = query.Id,
                Email = query.Email,
                IsUserActiveDirectory = query.IsActiveDirectory
            };

            //if (query.UserPassword.HashedPassword != (body.Password + query.UserPassword.Salt).ToSHA512())
            //    throw new BadRequestException("Username and Password not match");

            var impersonatedUser = new CheckUserNamePasswordImpersonateUserLoginAsResult
            {
                Id = impersonatedUserData.IdUser,
                UserName = impersonatedUserData.UserName,
                Email = impersonatedUserData.Email,
                IsUserActiveDirectory = impersonatedUserData.IsUserActiveDirectory
            };

            var isFeatureActive = await _featureManager.IsEnabledAsync(FeatureFlags.StudentBlocking);

            user = CreateResult(impersonatedUser, impersonatedUserData, isFeatureActive);

            return user;
        }

        public CheckUserNamePasswordImpersonateUserLoginAsResult CreateResult(CheckUserNamePasswordImpersonateUserLoginAsResult baseResult, ImpersonateUserLoginAsResult msUser, bool isFeatureActive)
        {
            baseResult.Email ??= msUser.Email;
            baseResult.UserName ??= msUser.UserName;
            baseResult.DisplayName ??= msUser.DisplayName;
            baseResult.Token = _tokenIssuer.IssueTokenForUser(
                new ClaimsToken
                {
                    UserId = msUser.IdUser,
                    UserName = msUser.UserName,
                    Roles = msUser.Roles.Select(role => new ClaimsRole
                    {
                        Id = role.Id,
                        Code = role.Code,
                        Name = role.Description
                    }),
                    Tenants = msUser.Schools.Select(school => new ClaimsTenant
                    {
                        Id = school.Id,
                        Name = school.Name
                    })
                },
                8);

            baseResult.Roles = msUser.Roles.Select(role => new GetDetailImpersonateUserResult_UserRoleResult
            {
                Id = role.Id,
                Name = role.Description,
                CanOpenTeacherTracking = role.CanOpenTeacherTracking,
                RoleGroup = new NameValueVm
                {
                    Id = role.RoleGroup.Id,
                    Name = role.RoleGroup.Name
                }
            });
            baseResult.Schools = msUser.Schools.Select(school => new GetDetailImpersonateUserResult_SchoolResult
            {
                Name = school.Name,
                Id = school.Id,
                Logo = ""
            });
            baseResult.ImpersonatorIdUser = AuthQuery.FirstOrDefault(x => x.Username == msUser.UserName).Id;

            //checking Student Blocking
            if (isFeatureActive)
            {
                if (baseResult.Roles.Any(x => x.RoleGroup.Name.ToUpper() == RoleConstant.Student))
                {
                    var dataStudentBLocking = GetDataBlocking(baseResult.Schools.Select(x => x.Id).SingleOrDefault(), msUser.IdUser);
                    baseResult.IsBlock = string.IsNullOrEmpty(dataStudentBLocking.Result) ? false : true;
                    baseResult.BlockingMessage = dataStudentBLocking.Result;
                }
                else
                {
                    baseResult.IsBlock = false;
                }
            }

            return baseResult;
        }

        protected async Task<string> GetDataBlocking(string idSchool, string idStudent)
        {
            var GeneratedData = "";
            if (!string.IsNullOrEmpty(idSchool) && !string.IsNullOrEmpty(idStudent))
            {
                var Message = await _dbContext.Entity<MsBlockingMessage>()
                .Where(e => e.IdSchool == idSchool)
                .Select(e => new
                {
                    e.Content,
                    e.IdCategory
                })
                .ToListAsync(CancellationToken);

                if (Message != null)
                {
                    var listMessage = new List<string>();
                    foreach (var msg in Message)
                    {
                        var pushTitle = Handlebars.Compile(msg.Content);

                        var dataStudentBlocking = await _dbContext.Entity<MsStudentBlocking>()
                                            .Include(x => x.BlockingCategory)
                                            .Include(x => x.BlockingType)
                                            .Where(x => x.IdStudent == idStudent
                                            && x.BlockingType.Category == "WEBSITE"
                                            && x.IsBlocked && x.IdBlockingCategory == msg.IdCategory)
                                            .Select(e => e.BlockingCategory.Name)
                                            .ToListAsync(CancellationToken);

                        if (dataStudentBlocking.Count != 0)
                        {
                            _DataMessageBlocking = new Dictionary<string, object>
                            {
                                { "categoryBlocking", string.Join(", ", dataStudentBlocking) },
                            };
                            listMessage.Add(pushTitle(_DataMessageBlocking));
                        }
                    }
                    GeneratedData = string.Join("<div style=\"padding-bottom: 25px;\"></div>", listMessage);
                }

            }
            return GeneratedData;
        }
    }
}
