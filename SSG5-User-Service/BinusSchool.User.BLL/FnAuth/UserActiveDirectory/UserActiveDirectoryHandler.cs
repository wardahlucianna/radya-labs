using System.Net.Http.Headers;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnAuth.UserActiveDirectory;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.User.FnAuth.UserActiveDirectory.Validator;
using BinusSchool.User.FnAuth.UserPassword;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using Microsoft.Graph;

namespace BinusSchool.User.FnAuth.UserActiveDirectory
{
    public class UserActiveDirectoryHandler : UserPasswordHandler
    {
        private readonly IFeatureManagerSnapshot _featureManager;
        public UserActiveDirectoryHandler(IUserDbContext dbContext, TokenIssuer tokenIssuer, [FromServices] IFeatureManagerSnapshot featureManager) : base(dbContext, tokenIssuer, featureManager) 
        {
            _featureManager = featureManager;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UserActiveDirectoryRequest, UserActiveDirectoryValidator>();
            var authProvider = new DelegateAuthenticationProvider(request =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", body.Token);
                return Task.CompletedTask;
            });

            // validate token with call Ms Graph
            var graphService = new GraphServiceClient(authProvider);
            var user = await graphService.Me.Request().GetAsync(CancellationToken);
            if (user.Mail is null)
                throw new NotFoundException("Email not found. Please contact Binus admin for login with Microsoft account.");

            // if validate token success, then query user by id
            var query = await AuthQuery.FirstOrDefaultAsync(x
                => x.IsActiveDirectory
                && x.Email == user.Mail, CancellationToken);
            if (query is null)
                throw new NotFoundException($"User {user.Mail} is not registered in this system.");
            if (!query.Status)
                throw new BadRequestException("Couldn't login, user is inactive");
            if (query.UserSchools.Count == 0)
                throw new BadRequestException($"School for user {query.Username} has not been set");
            if (query.UserRoles.Count == 0)
                throw new BadRequestException($"Role for user {query.Username} has not been set");

            var baseResult = new UserPasswordResult
            {
                Id = query.Id,
                Email = query.Email,
                DisplayName = query.DisplayName,
                UserName = query.Username,
                IsUserActiveDirectory = true
            };

            var isFeatureActive = await _featureManager.IsEnabledAsync(FeatureFlags.StudentBlocking);
            var result = CreateResult(baseResult, query, isFeatureActive);

            return Request.CreateApiResult2(result as object);
        }
    }
}
