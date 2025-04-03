using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IPersonalInvitation : IFnSchedule
    {
        #region Personal Invitation
        [Get("/schedule/personal-invitation")]
        Task <ApiErrorResult<IEnumerable<GetPersonalInvitationResult>>> GetPersonalInvitation(GetPersonalInvitationRequest query);

        [Post("/schedule/personal-invitation")]
        Task<ApiErrorResult> AddPersonalInvitation([Body] AddPersonalInvitationRequest body);

        [Post("/schedule/personal-invitation-v2")]
        Task<ApiErrorResult> AddPersonalInvitationV2([Body] AddPersonalInvitationRequest body);

        [Get("/schedule/personal-invitation/{id}")]
        Task<ApiErrorResult<DetailPersonalInvitationResult>> DetailPersonalInvitation(string id);

        [Put("/schedule/personal-invitation")]
        Task<ApiErrorResult> UpdatePersonalInvitation([Body] UpdatePersonalInvitationRequest body);

        [Delete("/schedule/personal-invitation")]
        Task<ApiErrorResult> DeletePersonalInvitation([Body] IEnumerable<string> body);

        [Get("/schedule/personal-invitation-availability-time")]
        Task<ApiErrorResult<IEnumerable<string>>> GetAvailabilityTimeTeacher(GetAvailabilityTimeTeacherRequest query);

        [Get("/schedule/personal-invitation-availability-time-v2")]
        Task<ApiErrorResult<IEnumerable<string>>> GetAvailabilityTimeTeacherV2(GetAvailabilityTimeTeacherRequest query);
        #endregion

        #region Personal Invitation Approval
        [Get("/schedule/personal-invitation-approval")]
        Task<ApiErrorResult<IEnumerable<GetPersonalInvitationApprovalResult>>> GetPersonalInvitationApproval(GetPersonalInvitationApprovalRequest query);

        [Post("/schedule/personal-invitation-approval")]
        Task<ApiErrorResult> UpdatePersonalInvitationApproval([Body] UpdatePersonalInvitationApprovalRequest body);

        [Put("/schedule/personal-invitation-cancel")]
        Task<ApiErrorResult> CancelPersonalInvitation([Body] CancelPersonalInvitationRequest body);
        #endregion
    }
}
