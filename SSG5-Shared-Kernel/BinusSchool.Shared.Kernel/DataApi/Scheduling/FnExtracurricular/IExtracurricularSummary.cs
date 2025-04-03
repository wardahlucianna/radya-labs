using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularSummary;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IExtracurricularSummary : IFnExtracurricular
    {
        [Get("/extracurricular-summary/get-extracurricular-summary-by-student")]
        Task<ApiErrorResult<IEnumerable<GetExtracurricularSummaryByStudentResult>>> GetExtracurricularSummaryByStudent(GetExtracurricularSummaryByStudentRequest body);
    }
}
