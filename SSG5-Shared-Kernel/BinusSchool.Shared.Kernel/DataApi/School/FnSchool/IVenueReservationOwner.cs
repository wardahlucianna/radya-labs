using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Floor;
using BinusSchool.Data.Model.School.FnSchool.VenueReservationOwner;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IVenueReservationOwner : IFnSchool
    {
        [Get("/school/venue-reservation-owner/get-ddl-venue-reservation-owner")]
        Task<ApiErrorResult<IEnumerable<NameValueVm>>> GetDDLVenueReservationOwner(GetDDLVenueReservationOwnerRequest param);
        [Post("/school/venue-reservation-owner/delete-pic")]
        Task<ApiErrorResult<IEnumerable<NameValueVm>>> DeletePIC([Body]DeletePICOwnerRequest body);
        [Post("/school/venue-reservation-owner/save-pic-owner")]
        Task<ApiErrorResult<IEnumerable<NameValueVm>>> SavePICOwner([Body] SavePicOwnerRequest body);
        [Get("/school/venue-reservation-owner/get-detail-pic")]
        Task<ApiErrorResult<GetDetailPICResult>> GetDetailPIC(GetDetailPICRequest param);
        [Get("/school/venue-reservation-owner/get-list-pic")]
        Task<ApiErrorResult<IEnumerable<GetListPICResult>>> GetListPICOwner(GetListPICRequest param);
    }
}
