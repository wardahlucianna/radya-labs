using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.BlockingType;
using Refit;

namespace BinusSchool.Data.Api.User.FnBlocking.UserBlocking
{
    public interface IBlockingType : IFnBlocking
    {
        [Get("/user-blocking/blocking-type")]
        Task<ApiErrorResult<IEnumerable<GetBlockingTypeResult>>> GetBlockingType(GetBlockingTypeRequest query);

        [Get("/user-blocking/blocking-type/detail/{id}")]
        Task<ApiErrorResult<GetBlockingTypeDetailResult>> GetBlockingTypeDetail(string id);

        [Post("/user-blocking/blocking-type")]
        Task<ApiErrorResult> AddBlockingType([Body] AddBlockingTypeRequest body);

        [Put("/user-blocking/blocking-type")]
        Task<ApiErrorResult> UpdateBlockingType([Body] UpdateBlockingTypeRequest body);

        [Delete("/user-blocking/blocking-type")]
        Task<ApiErrorResult> DeleteBlockingType([Body] IEnumerable<string> ids);

        [Get("/user-blocking/blocking-type/menu")]
        Task<ApiErrorResult<IEnumerable<GetBlockingTypeMenuResult>>> GetBlockingTypeMenu(GetBlockingTypeMenuRequest query);
          
    }

}
