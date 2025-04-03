using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueWithBuilding;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IVenueWithBuilding : IFnSchool
    {
        [Get("/school/venue-with-building")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetVenuesWithBuilding(GetVenueWithBuildingRequest query);
    }
}
