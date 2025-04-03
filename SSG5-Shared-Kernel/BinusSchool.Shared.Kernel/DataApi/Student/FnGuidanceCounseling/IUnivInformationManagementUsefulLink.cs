using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementUsefulLink;
using Refit;

namespace BinusSchool.Data.Api.Student.FnGuidanceCounseling
{
    public interface IUnivInformationManagementUsefulLink : IFnGuidanceCounseling
    {
        [Get("/guidance-counseling/university-information-management-useful-link")]
        Task<ApiErrorResult<IEnumerable<GetUnivInformationManagementUsefulLinkResult>>> GetListUnivInformationManagementUsefulLink(GetUnivInformationManagementUsefulLinkRequest query);

        [Get("/guidance-counseling/university-information-management-useful-link/detail/{id}")]
        Task<ApiErrorResult<GetUnivInformationManagementUsefulLinkResult>> GetUnivInformationManagementUsefulLinkDetail(string id);

        [Post("/guidance-counseling/university-information-management-useful-link")]
        Task<ApiErrorResult> AddUnivInformationManagementUsefulLink([Body] AddUnivInformationManagementUsefulLinkRequest body);

        [Put("/guidance-counseling/university-information-management-useful-link")]
        Task<ApiErrorResult> UpdateUnivInformationManagementUsefulLink([Body] UpdateUnivInformationManagementUsefulLinkRequest body);

        [Delete("/guidance-counseling/university-information-management-useful-link")]
        Task<ApiErrorResult> DeleteUnivInformationManagementUsefulLink([Body] IEnumerable<string> ids);
    }
}
