using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Student.FnGuidanceCounseling;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.UnivInformationManagementPortal;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.Student.FnGuidanceCounseling
{
    public interface IUnivInformationManagementPortal : IFnGuidanceCounseling
    {

        #region portal
        [Get("/guidance-counseling/university-information-management-portal")]
        Task<ApiErrorResult<IEnumerable<GetUnivInformationManagementPortalResult>>> GetListUnivInformationManagementPortal(GetUnivInformationManagementPortalRequest query);

        [Get("/guidance-counseling/university-information-management-portal/detail/{id}")]
        Task<ApiErrorResult<GetUnivInformationManagementPortalResult>> GetUnivInformationManagementPortalDetail(string id);

        [Post("/guidance-counseling/university-information-management-portal")]
        Task<ApiErrorResult> AddUnivInformationManagementPortal([Body] AddUnivInformationManagementPortalRequest body);

        [Put("/guidance-counseling/university-information-management-portal")]
        Task<ApiErrorResult> UpdateUnivInformationManagementPortal([Body] UpdateUnivInformationManagementPortalRequest body);

        [Delete("/guidance-counseling/university-information-management-portal")]
        Task<ApiErrorResult> DeleteUnivInformationManagementPortal([Body] IEnumerable<string> ids);
        #endregion

        #region portal-approval
        [Get("/guidance-counseling/university-information-management-portal-approval")]
        Task<ApiErrorResult<IEnumerable<GetUnivInformationManagementPortalApprovalResult>>> GetListUnivInformationManagementPortalApproval(GetUnivInformationManagementPortalApprovalRequest query);

        [Post("/guidance-counseling/university-information-management-portal-process-approval")]
        Task<ApiErrorResult> AddUnivInformationManagementPortalApproval([Body] AddUnivInformationManagementPortalApprovalRequest body);


        #endregion

    }
}
