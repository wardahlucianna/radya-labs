using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnAuth.ImpersonateLogin;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using Refit;

namespace BinusSchool.Data.Api.User.FnAuth
{
    public interface IImpersonateLogin : IFnAuth
    {
        [Post("/auth/impersonate-login")]
        Task<ApiErrorResult<ImpersonateLoginResult>> ImpersonateLogin([Body] ImpersonateLoginRequest body);

        [Post("/auth/impersonate-login/auth/up")]
        Task<ApiErrorResult<MCB01X7UserPasswordResult>> UserPasswordImpersonateLogin([Body] MCB01X7UserPasswordRequest body);
    }
}
