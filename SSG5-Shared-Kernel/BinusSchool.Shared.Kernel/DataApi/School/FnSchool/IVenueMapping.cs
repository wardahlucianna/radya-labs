using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.VenueMapping;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IVenueMapping : IFnSchool
    {
        [Get("/school/venue-mapping/get-venue-mapping")]
        Task<ApiErrorResult<IEnumerable<GetVenueMappingResult>>> GetVenueMapping(GetVenueMappingRequest query);

        [Post("/school/venue-mapping/save-venue-mapping")]
        Task<ApiErrorResult> SaveVenueMapping([Body] SaveVenueMappingRequest body);

        [Post("/school/venue-mapping/copy-venue-mapping")]
        Task<ApiErrorResult> CopyVenueMapping([Body] CopyVenueMappingRequest body);

        [Post("/school/venue-mapping/export-to-excel-venue-mapping")]
        Task<HttpResponseMessage> ExportToExcelVenueMapping([Body] ExportToExcelVenueMappingRequest body);

    }
}
