using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IElectiveCoach : IFnExtracurricular
    {
        [Get("/elective/coach")]
        Task<ApiErrorResult<IEnumerable<GetElectiveCoachResult>>> GetElectiveCoach(GetElectiveCoachRequest body);

        [Get("/elective/ext-coach/user")]
        Task<ApiErrorResult<IEnumerable<GetElectiveExternalCoachResult>>> GetElectiveExternalCoach(GetElectiveExternalCoachRequest body);

        [Get("/elective/ext-coach/user-detail")]
        Task<ApiErrorResult<GetElectiveExternalCoachDetailResult>> GetElectiveExternalCoachDetail(GetElectiveExternalCoachDetailRequest body);

        [Put("/elective/ext-coach/update-user")]
        Task<ApiErrorResult> UpdateElectiveExternalCoach([Body] UpdateElectiveExternalCoachRequest body);

        [Get("/elective/ext-coach/taxstatus")]
        Task<ApiErrorResult<IEnumerable<GetExtCoachTaxStatusResult>>> GetExtCoachTaxStatus();

        [Delete("/elective/ext-coach/delete-user")]
        Task<ApiErrorResult> DeleteElectiveExternalCoach([Body] DeleteElectiveExternalCoachRequest body);

    }
}
