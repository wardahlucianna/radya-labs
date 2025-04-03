using System.Linq;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Common.Functions.Extensions
{
    public static class HttpRequestExtension
    {
        public static AuthenticationInfo EnsureAnyUser(this HttpRequest request)
        {
            var authInfo = new AuthenticationInfo(request);
            
            return authInfo.IsValid 
                ? authInfo 
                : throw new UnauthorizeException(request.HttpContext.RequestServices.GetService<IStringLocalizer>()[authInfo.Message]);
        }

        public static AuthenticationInfo EnsureValidUser(this HttpRequest request, string shouldHaveRole, string shouldHavePermission = null)
        {
            var authInfo = request.EnsureAnyUser();
            var localizer = request.HttpContext.RequestServices.GetService<IStringLocalizer>();

            var userRole = authInfo.Roles.FirstOrDefault(role => role.Code == shouldHaveRole);
            if (userRole is null)
                throw new UnauthorizeException(string.Format(localizer["ExHasNoRole"], shouldHaveRole));
            else if (shouldHavePermission != null && !userRole.Permissions.Any(permission => permission.Name == shouldHavePermission))
                throw new UnauthorizeException(string.Format(localizer["ExHasNoPermission"], shouldHavePermission));

            return authInfo;
        }
    }
}
