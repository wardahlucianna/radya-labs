using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesScoreGrade;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IElectivesScoreGrade : IFnExtracurricular
    {
        [Get("/elective/elective-score-grade")]
        Task<ApiErrorResult<IEnumerable<GetElectivesScoreGradeResult>>> GetElectivesScoreGrade(GetElectivesScoreGradeRequest body);
    }
}
