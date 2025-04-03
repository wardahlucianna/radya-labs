using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Floor;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IFloor : IFnSchool
    {
        [Get("/school/floor/get-ddl-floor")]
        Task<ApiErrorResult<IEnumerable<GetDDLFloorResponse>>> GetDDLFloor(GetDDLFloorRequest param);

        [Get("/school/floor/get-list-floor")]
        Task<ApiErrorResult<IEnumerable<GetListFloorResult>>> GetListFloor(GetListFloorRequest param);

        [Get("/school/floor/get-detail-floor")]
        Task<ApiErrorResult<GetDetailFloorResult>> GetDetailFloor(GetDetailFloorRequest param);

        [Post("/school/floor/save-floor")]
        Task<ApiErrorResult> SaveFloor([Body] SaveFloorRequest body);

        [Post("/school/floor/delete-floor")]
        Task<ApiErrorResult> DeleteFloor([Body] DeleteFloorRequest body);

    }
}
