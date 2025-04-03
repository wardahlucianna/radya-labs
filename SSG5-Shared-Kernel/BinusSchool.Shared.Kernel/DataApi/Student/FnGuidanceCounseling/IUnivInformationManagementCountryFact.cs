using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementCountryFact;
using Refit;

namespace BinusSchool.Data.Api.Student.FnGuidanceCounseling
{
    public interface IUnivInformationManagementCountryFact : IFnGuidanceCounseling
    {
        [Get("/guidance-counseling/university-information-management-country-fact")]
        Task<ApiErrorResult<IEnumerable<GetUnivInformationManagementCountryFactResult>>> GetListUnivInformationManagementCountryFact(GetUnivInformationManagementCountryFactRequest query);

        [Get("/guidance-counseling/university-information-management-country-fact/detail/{id}")]
        Task<ApiErrorResult<GetUnivInformationManagementCountryFactResult>> GetUnivInformationManagementCountryFactDetail(string id);

        [Post("/guidance-counseling/university-information-management-country-fact")]
        Task<ApiErrorResult> AddUnivInformationManagementCountryFact([Body] AddUnivInformationManagementCountryFactRequest body);

        [Put("/guidance-counseling/university-information-management-country-fact")]
        Task<ApiErrorResult> UpdateUnivInformationManagementCountryFact([Body] UpdateUnivInformationManagementCountryFactRequest body);

        [Delete("/guidance-counseling/university-information-management-country-fact")]
        Task<ApiErrorResult> DeleteUnivInformationManagementCountryFact([Body] IEnumerable<string> ids);
    }
}
