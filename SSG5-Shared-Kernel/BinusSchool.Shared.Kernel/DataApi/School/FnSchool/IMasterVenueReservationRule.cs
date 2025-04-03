using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using System.Threading.Tasks;
using Refit;
using BinusSchool.Data.Model.School.FnSchool.MasterVenueReservationRule;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IMasterVenueReservationRule : IFnSchool
    {
        [Get("/school/master-venue-reservation-rule/get-master-venue-reservation-rule")]
        Task<ApiErrorResult<GetMasterVenueReservationRuleResult>> GetMasterVenueReservationRule(GetMasterVenueReservationRuleRequest query);

        [Post("/school/master-venue-reservation-rule/save-master-venue-reservation-rule")]
        Task<ApiErrorResult> SaveMasterVenueReservationRule([Body] SaveMasterVenueReservationRuleRequest body);
    }
}
