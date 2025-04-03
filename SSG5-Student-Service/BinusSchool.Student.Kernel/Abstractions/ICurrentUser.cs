namespace BinusSchool.Student.Kernel.Abstractions;

public interface ICurrentUser
{
    bool TryGetAuthorizationHeader(out string token);
    bool TryGetUser(out ClaimUser user);
    bool TryGetUser(out ClaimUser user, out string token);
    (ClaimUser user, string token) GetUserSystem();
}