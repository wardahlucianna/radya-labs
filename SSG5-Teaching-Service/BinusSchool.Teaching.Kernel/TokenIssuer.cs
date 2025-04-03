using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace BinusSchool.Teaching.Kernel;

/// <summary>
/// Wrapper class for encapsulating the token issuance logic.
/// </summary>
public class TokenIssuer
{
    private readonly IJwtAlgorithm _algorithm;
    private readonly IJsonSerializer _serializer;
    private readonly IBase64UrlEncoder _base64Encoder;
    private readonly IJwtEncoder _jwtEncoder;

    public TokenIssuer()
    {
        // JWT specific initialization.
        // https://github.com/jwt-dotnet/jwt
        _algorithm = new HMACSHA256Algorithm();
        _serializer = new JsonNetSerializer();
        _base64Encoder = new JwtBase64UrlEncoder();
        _jwtEncoder = new JwtEncoder(_algorithm, _serializer, _base64Encoder);
    }

    /// <summary>
    /// This method is intended to be the main entry point for generation of the JWT.
    /// </summary>
    /// <param name="credentials">The user that the token is being issued for.</param>
    /// <returns>A JWT token which can be returned to the user.</returns>
    public string IssueTokenForUser(ClaimsToken credentials, int ExpHourse)
    {
        //var provider = new UtcDateTimeProvider();
        //var now = provider.GetNow().AddSeconds(ExpTsecond);
        //var secondsSinceEpoch = UnixEpoch.GetSecondsSince(now); //for get time expired
        //var secondsSinceEpoch = DateTimeOffset.UtcNow.AddHours(ExpHourse).ToUnixTimeSeconds(); //for get time expired

        // Instead of returning a string, we'll return the JWT with a set of claims about the user
        Dictionary<string, object> claims = new Dictionary<string, object>
        {
            // JSON representation of the user Reference with ID and display name
            { "name", credentials.UserName },

            { "sid", credentials.UserId },
            { "email", credentials.Email },
            { "role", credentials.Roles },
            { "tenant", credentials.Tenants },
            //{ "exp", secondsSinceEpoch },
        };

        string token = _jwtEncoder.Encode(claims, JwtConstant.SECRET_KEY);

        return token;
    }
}