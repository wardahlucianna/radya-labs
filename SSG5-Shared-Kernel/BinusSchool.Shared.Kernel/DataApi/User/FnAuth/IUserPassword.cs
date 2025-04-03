using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using Refit;

namespace BinusSchool.Data.Api.User.FnAuth
{
    public interface IUserPassword : IFnAuth
    {
        [Post("/auth/up")]
        Task<ApiErrorResult<UserPasswordResult>> UserPassword([Body] UserPasswordRequest body);
    }
}