using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BinusSchool.User.Kernel;

public static class HttpRequestExtensions
{
    public static AuthenticationInfo ToAuthenticationInfo(this HttpRequest request) => new(request);

    public static IActionResult ReturnUnauthorizedResult(this AuthenticationInfo _,
        HttpRequest request)
        => new UnauthorizedObjectResult(ApiResultExt.CreateError(request.Path.Value, "Unauthorized"));
}