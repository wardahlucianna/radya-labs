using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Shared.Kernel.DataModel.Student.FnStudent.StudentOperation.StudentUnderAttention;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Shared.Kernel.DataApi.Student.FnStudent
{
    public interface IStudentOperation : IFnStudent
    {
        #region Student Under Attention
        [Get("/student/student-operation/student-under-attention/student-status-special")]
        Task<ApiErrorResult<IEnumerable<GetStudentUnderAttentionStudentStatusSpecialResponse>>> GetStudentUnderAttentionStudentStatusSpecial();

        [Get("/student/student-operation/student-under-attention")]
        Task<ApiErrorResult<IEnumerable<GetStudentUnderAttentionResponse>>> GetStudentUnderAttention(GetStudentUnderAttentionRequest request);

        [Get("/student/student-operation/student-under-attention/future-admission-decision-form")]
        Task<ApiErrorResult<GetStudentUnderAttentionFutureAdmissionDecisionFormResponse>> GetStudentUnderAttentionFutureAdmissionDecisionForm(GetStudentUnderAttentionFutureAdmissionDecisionFormRequest request);

        [Post("/student/student-operation/student-under-attention/save-future-admission-decision-form")]
        Task<ApiErrorResult> SaveStudentUnderAttentionFutureAdmissionDecisionForm([Body] SaveStudentUnderAttentionFutureAdmissionDecisionFormRequest request);
        #endregion
    }
}
