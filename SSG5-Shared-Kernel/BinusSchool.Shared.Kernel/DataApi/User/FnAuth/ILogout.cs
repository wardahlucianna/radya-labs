using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnAuth.Logout;
using Refit;

namespace BinusSchool.Data.Api.User.FnAuth
{
    public interface ILogout : IFnAuth
    {
        [Post("/logout")]
        Task<ApiErrorResult> Logout([Body] LogoutRequest body);
    }
}
