using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.EmailInvitation;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IEmailInvitation : IFnSchedule
    {

        [Get("/schedule/email-invitation")]
        Task<ApiErrorResult<IEnumerable<GetEmailInvitationResult>>> GetEmailInvitation(GetEmailInvitationRequest query);

        [Post("/schedule/email-invitation")]
        Task<ApiErrorResult> AddEmailInvitation([Body] AddEmailInvitationRequest body);


        [Put("/schedule/email-invitation")]
        Task<ApiErrorResult> SendEmailInvitation([Body] SendEmailInvitationRequest body);

        [Delete("/schedule/email-invitation")]
        Task<ApiErrorResult> DeleteEmailInvitation([Body] IEnumerable<string> body);
        [Get("/schedule/email-invitation-student")]
        Task<ApiErrorResult<IEnumerable<GetHomeroomStudentResult>>> GetStudentByEmailInvitation(GetStudentByEmailInvitationRequest query);

    }
}
