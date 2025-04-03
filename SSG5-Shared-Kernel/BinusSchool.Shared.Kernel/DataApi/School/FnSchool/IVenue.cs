using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Venue;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IVenue : IFnSchool
    {
        [Get("/school/venue")]
        Task<ApiErrorResult<IEnumerable<GetVenueResult>>> GetVenues(GetVenueRequest query);

        [Get("/school/venue/{id}")]
        Task<ApiErrorResult<GetVenueDetailResult>> GetVenueDetail(string id);

        [Get("/school/venue-by-code")]
        Task<ApiErrorResult<List<GetVenueForAscTimetableResult>>> GetVenueListForAsc([Query] GetVenueForAscTimetableRequest request);

        [Post("/school/venue")]
        Task<ApiErrorResult> AddVenue([Body] AddVenueRequest body);

        [Put("/school/venue")]
        Task<ApiErrorResult> UpdateVenue([Body] UpdateVenueRequest body);

        [Delete("/school/venue")]
        Task<ApiErrorResult> DeleteVenue([Body] IEnumerable<string> ids);
    }
}
