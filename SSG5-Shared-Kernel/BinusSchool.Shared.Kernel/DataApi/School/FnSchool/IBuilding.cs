using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Building;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IBuilding : IFnSchool
    {
        [Get("/school/build")]
        Task<ApiErrorResult<IEnumerable<ListBuildingResponse>>> GetBuildings(CollectionSchoolRequest query);

        [Get("/school/build/{id}")]
        Task<ApiErrorResult<DetailResult2>> GetBuildingDetail(string id);

        [Post("/school/build")]
        Task<ApiErrorResult> AddBuilding([Body] AddBuildingRequest body);

        [Put("/school/build")]
        Task<ApiErrorResult> UpdateBuilding([Body] UpdateBuildingRequest body);

        [Delete("/school/build")]
        Task<ApiErrorResult> DeleteBuilding([Body] IEnumerable<string> ids);
    }
}
