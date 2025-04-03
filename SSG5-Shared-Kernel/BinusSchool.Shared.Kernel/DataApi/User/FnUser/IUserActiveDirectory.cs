using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.UserActiveDirectory;
using Refit;

namespace BinusSchool.Data.Api.User.FnUser
{
    public interface IUserActiveDirectory : IFnUser
    {
        [Get("/user-ad")]
        Task<ApiErrorResult<IEnumerable<GetUserActiveDirectoryResult>>> GetUserActiveDirectories(GetUserActiveDirectoryRequest request);
    }
}
