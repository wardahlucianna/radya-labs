using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSettings.SubjectAliasSettings;
using System.Net.Http;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ISubjectAliasSettings : IFnScoring
    {
        [Get("/score-settings/get-subject-alias-settings-list")]
        Task<ApiErrorResult<IEnumerable<GetSubjectAliasSettingsListResult>>> GetSubjectAliasSettingsList(GetSubjectAliasSettingsListRequest query);

        [Post("/score-settings/add-subject-alias-settings")]
        Task<HttpResponseMessage> AddSubjectAliasSettings([Body] AddUpdateSubjectAliasSettingsRequest query);

        [Put("/score-settings/update-subject-alias-settings")]
        Task<HttpResponseMessage> UpdateSubjectAliasSettings([Body] AddUpdateSubjectAliasSettingsRequest query);

        [Delete("/score-settings/delete-subject-alias-settings")]
        Task<HttpResponseMessage> DeleteSubjectAliasSettings([Body] IEnumerable<string> ids);

        [Get("/score-settings/get-subject-alias-settings-detail/{id}")]
        Task<ApiErrorResult<GetSubjectAliasSettingsDetailResult>> GetSubjectAliasSettingsDetail(string id);
    }
}
