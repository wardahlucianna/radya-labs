using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnAuth.UserActiveDirectory;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using Refit;

namespace BinusSchool.Data.Api.User.FnAuth
{
    public interface IUserActiveDirectory : IFnAuth
    {
        [Post("/auth/ad")]
        Task<ApiErrorResult<UserPasswordResult>> UserActiveDirectory([Body] UserActiveDirectoryRequest body);
    }
}
