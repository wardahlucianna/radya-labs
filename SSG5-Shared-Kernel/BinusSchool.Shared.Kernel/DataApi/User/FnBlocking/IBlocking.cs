using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using Refit;

namespace BinusSchool.Data.Api.User.FnBlocking.UserBlocking
{
    public interface IBlocking : IFnBlocking
    {
        [Get("/user-blocking/blocking")]
        Task<ApiErrorResult<GetBlockingResult>> GetBlocking(GetBlockingRequest query);

        [Get("/user-blocking/blocking-list")]
        Task<ApiErrorResult<IEnumerable<GetListBlockingResult>>> GetListBlocking(GetListBlockingRequest query);

        [Get("/user-blocking/blocking-dashboard")]
        Task<ApiErrorResult<GetBlockingDashboardResult>> GetBlockingDashboard(GetBlockingDashboardRequest query);
    }
}
