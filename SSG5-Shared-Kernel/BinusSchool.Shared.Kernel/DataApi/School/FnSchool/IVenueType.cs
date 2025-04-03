using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueType;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IVenueType : IFnSchool
    {
        [Get("/school/venue-type/get-ddl-venue-type")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetDDLVenueType(GetDDLVenueTypeRequest param);

        [Get("/school/venue-type/get-list-venue-type")]
        Task<ApiErrorResult<IEnumerable<GetListVenueTypeResult>>> GetVenueType(GetListVenueTypeRequest param);

        [Post("/school/venue-type/save-venue-type")]
        Task<ApiErrorResult> SaveVenueType([Body] SaveVenueTypeRequest body);

        [Post("/school/venue-type/delete-venue-type")]
        Task<ApiErrorResult> DeleteVenueType([Body] DeleteVenueTypeRequest body);

        [Get("/school/venue-type/get-detail-venue-type")]
        Task<ApiErrorResult<GetDetailVenueTypeResult>> GetDetailVenueType(GetDetailVenueTypeRequest param);
    }
}
