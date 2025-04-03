using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.Register;
using Refit;

namespace BinusSchool.Data.Api.User.FnUser
{
    public interface IRegister : IFnUser
    {
        [Post("/register/push-token")]
        Task<ApiErrorResult> RegisterPushToken([Body] RegisterPushTokenRequest body);

        [Post("/register/get-firebase-token")]
        Task<ApiErrorResult<List<GetFirebaseTokenResult>>> GetFirebaseToken([Body] GetFirebaseTokenRequest body);
    }
}
