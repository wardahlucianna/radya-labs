using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.OnlineRegistration;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IOnlineRegistration : IFnExtracurricular
    {
        [Post("/scp-online-registration/get-registration-detail")]
        Task<ApiErrorResult<IEnumerable<GetRegistrationDetailResult>>> GetRegistrationDetail([Body] GetRegistrationDetailRequest body);

        [Post("/scp-online-registration/get-extracurricular-list-by-student")]
        Task<ApiErrorResult<IEnumerable<GetExtracurricularListByStudentResult>>> GetExtracurricularListByStudent([Body] GetExtracurricularListByStudentRequest body);

        [Post("/scp-online-registration/save-student-extracurricular")]
        Task<ApiErrorResult<SaveStudentExtracurricularResult>> SaveStudentExtracurricular([Body] SaveStudentExtracurricularRequest body);

        [Post("/scp-online-registration/get-active-grade-by-students")]
        Task<ApiErrorResult<IEnumerable<GetActiveStudentsGradeByStudentResult>>> GetActiveStudentsGradeByStudent([Body] GetActiveStudentsGradeByStudentRequest body);

        [Get("/scp-online-registration/get-support-doc")]
        Task<ApiErrorResult<IEnumerable<GetSupportingDucumentByStudentResult>>> GetSupportingDucumentByStudent(GetSupportingDucumentByStudentRequest body);
    }
}
