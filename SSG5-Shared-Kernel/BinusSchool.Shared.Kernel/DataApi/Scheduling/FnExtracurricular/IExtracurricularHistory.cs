using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularHistory;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IExtracurricularHistory : IFnExtracurricular
    {
        [Get("/extracurricular-history/get-student-extracurricular-history-list")]
        Task<ApiErrorResult<IEnumerable<GetStudentExtracurricularHistoryListResult>>> GetStudentExtracurricularHistoryList(GetStudentExtracurricularHistoryListRequest body);
    }
}
