using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.UserRole;
using Refit;

namespace BinusSchool.Data.Api.User.FnUser
{
   
    public interface IUserRole : IFnUser 
    {
        [Post("/userrole/add-by-rolecode")]
        Task<ApiErrorResult> AddUserRoleByRoleCode([Body] AddUserRoleByRoleCodeRequest body);

        [Put("/userrole/update-all-user-role")]
        Task<ApiErrorResult> UpdateAllUserRoleByRoleCode([Body] UpdateAllUserRoleByRoleCodeRequest body);
    }

}
