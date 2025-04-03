using BinusSchool.Util.Kernel.Abstractions;
using Microsoft.AspNetCore.Http;

namespace BinusSchool.Util.Kernel;

public class CurrentUser : ICurrentUser
{
    public IHttpContextAccessor HttpContextAccessor => _httpContextAccessor;

    private static readonly ClaimUser _userSystem = new ClaimUser
    {
        Id = Guid.Empty.ToString(),
        Name = "SYSTEM",
        Email = "system@binus.edu"
    };

    private static readonly ClaimsToken _tokenSystem = new ClaimsToken
    {
        UserId = _userSystem.Id,
        UserName = _userSystem.Name,
        Email = _userSystem.Email
    };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TokenIssuer _tokenIssuer;

    public CurrentUser(IHttpContextAccessor httpContextAccessor, TokenIssuer tokenIssuer)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenIssuer = tokenIssuer;
    }

    public bool TryGetUser(out ClaimUser user)
    {
        return TryGetUser(out user, out _);
    }

    public bool TryGetAuthorizationHeader(out string token)
    {
        token = default;

        if (_httpContextAccessor.HttpContext is null)
            return false;
        if (!_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out var tokenValues))
            return false;

        token = tokenValues.FirstOrDefault();

        return true;
    }

    public bool TryGetUser(out ClaimUser user, out string token)
    {
        user = default;
        token = default;

        if (_httpContextAccessor.HttpContext is null)
            return false;
        if (!TryGetAuthorizationHeader(out token))
            return false;

        var authInfo = new AuthenticationInfo(_httpContextAccessor.HttpContext.Request);

        if (authInfo.IsValid)
        {
            user = new ClaimUser
            {
                Id = authInfo.UserId,
                Name = authInfo.UserName,
                Email = authInfo.Email
            };

            return true;
        }

        return false;
    }

    public (ClaimUser user, string token) GetUserSystem()
    {
        var token = _tokenIssuer.IssueTokenForUser(_tokenSystem, 1);

        return (_userSystem, token);
    }
}