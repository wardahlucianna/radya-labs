using BinusSchool.Common.Model;
using System.Threading.Tasks;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using Refit;
using BinusSchool.Data.Model.School.FnSurveyNoSql.MySurvey;

namespace BinusSchool.Data.Api.School.FnSurveyNoSql
{
    public interface IMySurveyNoSql : IFnSurveyNoSql
    {
        [Post("/answer")]
        Task<ApiErrorResult> AddAnswerRespondentNoSql([Body] AddAnswerRespondentNoSqlRequest body);

        [Get("/answer?id={id}")]
        Task<ApiErrorResult<GetAnswerRespondentNoSqlDetailResult>> DetailAnswerRespondentNoSql(string id);
    }
}
