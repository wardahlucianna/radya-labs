using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.EmailRecepient;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnMovingStudent
{
    public interface IEmailRecepient : IFnMovingStudent
    {

        [Get("/moving-student/email-recepient")]
        Task<ApiErrorResult<IEnumerable<GetEmailRecepientResult>>> GetEmailRecepient(GetEmailRecepientRequest query);

        [Post("/moving-student/email-recepient")]
        Task<ApiErrorResult> AddEmailRecepient([Body] AddEmailRecepientRequest query);

        [Get("/moving-student/email-recepient-to-and-bcc")]
        Task<ApiErrorResult<GetEmailBccAndTosResult>> GetEmailBccAndTos(GetEmailBccAndTosRequest query);

    }
}
