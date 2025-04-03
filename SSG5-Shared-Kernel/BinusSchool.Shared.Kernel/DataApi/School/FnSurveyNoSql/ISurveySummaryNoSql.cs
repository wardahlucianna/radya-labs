using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.School.FnSurveyNoSql.SurveySummary;

namespace BinusSchool.Data.Api.School.FnSurveyNoSql
{
    public interface ISurveySummaryNoSql : IFnSurveyNoSql
    {
        [Post("/survey-summary")]
        Task<ApiErrorResult<List<GetSurveySummaryNoSqlResult>>> GetSurveySummaryNoSql([Body] GetSurveySummaryNoSqlRequest body);
    }
}
