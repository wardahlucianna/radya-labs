using System.Collections.Generic;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;
using System.Net.Http;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IPublishSurvey : IFnSchool
    {

        #region publish Survey
        [Get("/publish-survey")]
        Task<ApiErrorResult<IEnumerable<GetPublishSurveyResult>>> GetPublishSurvey(GetPublishSurveyRequest query);

        [Post("/publish-survey")]
        Task<ApiErrorResult<AddPublishSurveyResult>> AddPublishSurvey([Body] AddPublishSurveyRequest body);

        [Get("/publish-survey/{id}")]
        Task<ApiErrorResult<DetailPublishSurveyResult>> DetailPublishSurvey(string id);

        [Put("/publish-survey")]
        Task<ApiErrorResult> UpdatePublishSurvey([Body] UpdatePublishSurveyRequest body);

        [Get("/publish-survey-unpublish")]
        Task<ApiErrorResult> UnpublishSurvey(UnpublishSurveyRequest body);
        #endregion

        #region Student Learning Survey
        [Get("/publish-survey-reset-mapping-student-leraning")]
        Task<ApiErrorResult<GetResetMappingStudentLearningSurveyResult>> GetResetMappingStudentLearningSurvey(GetResetMappingStudentLearningSurveyRequest query);

        [Get("/publish-survey-mapping-student-leraning")]
        Task<ApiErrorResult<GetResetMappingStudentLearningSurveyResult>> GetMappingStudentLearningSurvey(GetResetMappingStudentLearningSurveyRequest query);

        [Get("/publish-survey-homeroom-mapping-student-leraning")]
        Task<ApiErrorResult<GetHomeroomMappingStudentLearningSurveyResult>> GetHomeroomMappingStudentLearningSurvey(GetHomeroomMappingStudentLearningSurveyRequest query);

        [Post("/publish-survey-mapping")]
        Task<ApiErrorResult> AddMappingStudentLearningSurvey([Body] AddMappingStudentLearningSurveyRequest body);

        [Get("/publish-survey-download-mapping-student-learning")]
        Task<HttpResponseMessage> DownloadMappingStudentLearningSurvey(DownloadMappingStudentLearningSurveyRequest query);
        #endregion

        #region MandatorySurveyUser
        [Get("/publish-survey-mandatory-survey-user")]
        Task<ApiErrorResult<IEnumerable<object>>> GetMandatorySurveyUser(GetSurveyMandatoryUserRequest query);
        #endregion
    }
}
