using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Auth.Authentications.Jwt.ModelClaims;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.User.FnAuth.UserPassword.Validator;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

namespace BinusSchool.User.FnAuth.UserPassword
{
    public class UserPasswordHandler : FunctionsHttpSingleHandler
    {
        protected readonly IQueryable<MsUser> AuthQuery;

        private IDictionary<string, object> _DataMessageBlocking;
        private readonly TokenIssuer _tokenIssuer;
        private readonly IUserDbContext _userDbContex;
        private readonly IFeatureManagerSnapshot _featureManager;

        public UserPasswordHandler(IUserDbContext userDbContext, TokenIssuer tokenIssuer, [FromServices] IFeatureManagerSnapshot featureManager)
        {
            AuthQuery = userDbContext.Entity<MsUser>()
                .Include(x => x.UserPassword)
                .Include(x => x.UserSchools).ThenInclude(x => x.School)
                .Include(x => x.UserRoles).ThenInclude(x => x.Role).ThenInclude(x => x.RoleGroup);
            
            _tokenIssuer = tokenIssuer;

            _userDbContex = userDbContext;

            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UserPasswordRequest, UserPasswordValidator>();

            var query = await AuthQuery.Include(x => x.UserPassword)
                .FirstOrDefaultAsync(x => x.Username == body.UserName, CancellationToken);
            if (query is null)
                throw new NotFoundException("Invalid Login (error code: 01)");
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

            var user = new UserPasswordResult
            {
                Id = query.Id,
                Email = query.Email,
                IsUserActiveDirectory = query.IsActiveDirectory
            };
            
            if (!query.IsActiveDirectory)
            {
                
                if (query.UserPassword.HashedPassword != (body.Password + query.UserPassword.Salt).ToSHA512())
                    throw new BadRequestException("Invalid Login (error code: 06)");

                var isFeatureActive = await _featureManager.IsEnabledAsync(FeatureFlags.StudentBlocking);
                user = CreateResult(user, query, isFeatureActive);
            }

            return Request.CreateApiResult2(user as object);
        }

        protected UserPasswordResult CreateResult(UserPasswordResult baseResult, MsUser msUser,bool isFeatureActive)
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
            baseResult.Roles = msUser.UserRoles.OrderByDescending(role => role.IsDefault).Select(role => new UserRoleResult
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
            });
            baseResult.Schools = msUser.UserSchools.Select(school => new SchoolResult
            {
                Name = school.School.Name,
                Id = school.School.Id,
                Logo = ""
            });
           
            //checking Student Blocking
            if (isFeatureActive)
            {
                if (baseResult.Roles.Any(x => x.RoleGroup.Name.ToUpper() == RoleConstant.Student))
                {
                    var dataStudentBLocking = GetDataBlocking(baseResult.Schools.Select(x => x.Id).SingleOrDefault(), msUser.Id);
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
                var MessageData = await _userDbContex.Entity<MsBlockingMessage>()
                .Where(e => e.IdSchool == idSchool)
                .OrderBy(x=> x.DateIn)
                .ToListAsync(CancellationToken);

                if (MessageData.Count > 0)
                {
                    var dataBlocking = new List<string>();
                    foreach (var Message in MessageData)
                    {
                        if (Message.IdCategory != null)
                        {
                            var dataStudentBlocking = await _userDbContex.Entity<MsStudentBlocking>()
                            .Include(x => x.BlockingCategory)
                            .Include(x => x.BlockingType)
                            .Where(x => x.IdStudent == idStudent && x.BlockingType.Category == "WEBSITE" && x.IsBlocked 
                                        && x.IdBlockingCategory == Message.IdCategory)
                            .OrderBy(x => x.DateIn)
                            .Select(e => e.BlockingCategory.Name)
                            .FirstOrDefaultAsync(CancellationToken);

                            var pushTitle = Handlebars.Compile(Message.Content);


                            if (dataStudentBlocking != null)
                            {
                                _DataMessageBlocking = new Dictionary<string, object>
                                {
                                    { "categoryBlocking", string.Join(", ", dataStudentBlocking) },
                                };
                                GeneratedData += pushTitle(_DataMessageBlocking);
                            }
                        }
                    }
                }

            }
            return GeneratedData;
        }
    }
}
