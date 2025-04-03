using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueEquipment;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IVenueEquipment : IFnSchool
    {
        [Post("/school/venue-equipment/delete-venue-equipment")]
        Task<ApiErrorResult<IEnumerable<NameValueVm>>> DeleteVenueEquipment([Body] DeleteVenueEquipmentRequest body);
        [Post("/school/venue-equipment/save-venue-equipment")]
        Task<ApiErrorResult<IEnumerable<NameValueVm>>> SaveVenueEquipment([Body] SaveVenueEquipRequest body);
        [Get("/school/venue-equipment/get-venue-equipment-detail")]
        Task<ApiErrorResult<GetVenueEquipmentDetailResult>> GetVenueEquipmentDetail(GetVenueEquipmentDetailRequest param);
        [Get("/school/venue-equipment/get-list-venue-equipment")]
        Task<ApiErrorResult<IEnumerable<GetListVenueEquipmentResult>>> GetListVenueEquipment(GetListVenueEquipmentRequest param);
        [Get("/school/venue-equipment/get-list-select-equipment")]
        Task<ApiErrorResult<IEnumerable<GetListSelectEquipmentResult>>> GetListSelectEquipment(GetListSelectEquipmentRequest param);
    }
}
