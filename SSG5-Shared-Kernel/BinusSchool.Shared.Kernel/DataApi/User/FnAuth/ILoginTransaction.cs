using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnAuth.LoginTransaction;
using Refit;

namespace BinusSchool.Data.Api.User.FnAuth
{
    public interface ILoginTransaction : IFnAuth
    {
        [Post("/auth/login-transaction")]
        Task<ApiErrorResult> AddLoginTransaction([Body] AddLoginTransactionRequest body);
    }
}
