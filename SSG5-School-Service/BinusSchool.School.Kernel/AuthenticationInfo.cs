using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BinusSchool.School.Kernel;

public class AuthenticationInfo : ClaimsToken
{
    private readonly Lazy<IEnumerable<ClaimsRole>> _roles;
    private readonly Lazy<IEnumerable<ClaimsTenant>> _tenants;
    private readonly string _idCurrentTenant;

    public bool IsValid { get; }
    public string Message { get; }
    public string IdCurrentTenant => _idCurrentTenant ?? Tenants.First().Id;

    public AuthenticationInfo(HttpRequest request, bool useLocalizeMessage = true)
    {
        // Check if we have authorization header & have value
        if (!request.Headers.ContainsKey("Authorization"))
        {
            Message = useLocalizeMessage ? "ExHeaderNoAuth" : "Invalid request header.";
            return;
        }

        string authorizationHeader = request.Headers["Authorization"][0];

        // Check if we can decode the header.
        IDictionary<string, object> claims;

        try
        {
            if (authorizationHeader.StartsWith("Bearer"))
            {
                authorizationHeader = authorizationHeader.Substring(7);
            }

            // Validate the token and decode the claims.
            claims = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(JwtConstant.SECRET_KEY)
                .MustVerifySignature()
                .Decode<IDictionary<string, object>>(authorizationHeader);
        }
        catch (Exception ex)
        {
            Message = ex switch
            {
                TokenExpiredException _ => useLocalizeMessage ? "ExTokenExpire" : "Token has expired.",
                SignatureVerificationException _ => useLocalizeMessage
                    ? "ExTokenInvalidSignature"
                    : "Token has invalid signature.",
                _ => ex.Message
            };

            return;
        }

        // Check if we have user claim.
        if (!claims.ContainsKey("sid"))
        {
            Message = useLocalizeMessage ? "ExClaimsNoIdentity" : "No identity key was found in the claims.";
            return;
        }

        // Get current school id
        if (request.Headers.TryGetValue("X-Current-Tenant", out var currentSchools))
            _idCurrentTenant = currentSchools.FirstOrDefault();

        IsValid = true;
        UserId = Convert.ToString(claims["sid"]);
        UserName = Convert.ToString(claims["name"]);
        Email = Convert.ToString(claims["email"]);

        if (claims.TryGetValue("role", out var roles) && roles != null)
            _roles = new Lazy<IEnumerable<ClaimsRole>>(() =>
                JsonConvert.DeserializeObject<IEnumerable<ClaimsRole>>(roles!.ToString()!));
        if (claims.TryGetValue("tenant", out var tenants) && tenants != null)
            _tenants = new Lazy<IEnumerable<ClaimsTenant>>(() =>
                JsonConvert.DeserializeObject<IEnumerable<ClaimsTenant>>(tenants!.ToString()!));

        Roles = _roles?.Value;
        Tenants = _tenants?.Value;
    }
}