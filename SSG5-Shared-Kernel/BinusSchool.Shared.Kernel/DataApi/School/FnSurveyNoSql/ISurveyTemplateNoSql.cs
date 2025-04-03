using BinusSchool.Common.Model;
using System.Threading.Tasks;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using Refit;
using BinusSchool.Data.Model.School.FnSurveyNoSql.SurveyTemplate;

namespace BinusSchool.Data.Api.School.FnSurveyNoSql
{
    public interface ISurveyTemplateNoSql : IFnSurveyNoSql
    {
        [Post("/survey-template")]
        Task<ApiErrorResult> AddSurveyTemplateNoSql([Body] AddSurveyTemplateNoSqlRequest body);

        [Get("/survey-template?id={id}")]
        Task<ApiErrorResult<AddSurveyTemplateNoSqlRequest>> DetailSurveyTemplateNoSql(string id);
    }
}
