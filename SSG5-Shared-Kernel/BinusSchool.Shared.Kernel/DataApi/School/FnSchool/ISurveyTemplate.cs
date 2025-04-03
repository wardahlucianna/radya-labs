using System.Collections.Generic;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.School.FnSchool.SurveyTemplate;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface ISurveyTemplate : IFnSchool
    {
        [Get("/survey-template")]
        Task<ApiErrorResult<IEnumerable<GetSurveyTemplateResult>>> GetSurveyTemplate(GetSurveyTemplateRequest param);

        [Post("/survey-template")]
        Task<ApiErrorResult> AddSurveyTemplate([Body] AddSurveyTemplateRequest body);

        [Get("/survey-template/{id}")]
        Task<ApiErrorResult<List<DetailSurveyTemplateResult>>> DetailSurveyTemplate(string id);

        [Put("/survey-template")]
        Task<ApiErrorResult> UpdateSurveyTemplate([Body] AddSurveyTemplateRequest body);

        [Delete("/survey-template")]
        Task<ApiErrorResult> DeleteSurveyTemplate([Body] IEnumerable<string> body);

        [Get("/survey-template-copy")]
        Task<ApiErrorResult<IEnumerable<GetSurveyTemplateCopyResult>>> GetSurveyTemplateCopy(GetSurveyTemplateCopyRequest param);

        [Post("/survey-template-copy")]
        Task<ApiErrorResult> AddSurveyTemplateCopy([Body] AddSurveyTemplateCopyRequest body);
    }
}
