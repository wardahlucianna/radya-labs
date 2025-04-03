using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using Refit;

namespace BinusSchool.Data.Api.School.FnSchool
{
    public interface IMeritDemerit : IFnSchool
    {
        #region component-setting
        [Get("/school/merit-demerit/component-setting")]
        Task<ApiErrorResult<IEnumerable<GetMeritDemeritComponentSettingResult>>> GetMeritDemeritComponentSetting(GetMeritDemeritComponentSettingRequest query);

        [Put("/school/merit-demerit/component-setting")]
        Task<ApiErrorResult> UpdateMeritDemeritComponentSetting([Body] UpdateMeritDemeritComponentSettingRequest body);
        #endregion

        #region discipline-aproval
        [Get("/school/merit-demerit/discipline-aproval")]
        Task<ApiErrorResult<IEnumerable<GetMeritDemeritDisciplineAprovalResult>>> GetMeritDemeritDisciplineAproval(GetMeritDemeritDisciplineAprovalRequest query);

        [Put("/school/merit-demerit/discipline-aproval")]
        Task<ApiErrorResult> UpdateMeritDemeritDisciplineAproval([Body] UpdateMeritDemeritDisciplineAprovalRequest body);
        #endregion

        #region discipline-mapping
        [Post("/school/merit-demerit/discipline-mapping")]
        Task<ApiErrorResult> AddMeritDemeritDisciplineMap([Body] AddMeritDemeritDisciplineMappingRequest body);

        [Get("/school/merit-demerit/discipline-mapping")]
        Task<ApiErrorResult<IEnumerable<GetMeritDemeritDisciplineMappingResult>>> GetMeritDemeritDisciplineMapping(GetMeritDemeritDisciplineMappingRequest query);

        [Put("/school/merit-demerit/discipline-mapping")]
        Task<ApiErrorResult> UpdateMeritDemeritDisciplineMapping([Body] UpdateMeritDemeritDisciplineMappingRequest body);

        [Delete("/school/merit-demerit/discipline-mapping")]
        Task<ApiErrorResult> DeleteMeritDemeritDisciplineMapping([Body] IEnumerable<string> body);

        [Get("/school/merit-demerit/discipline-mapping/{id}")]
        Task<ApiErrorResult<GetMeritDemeritDisciplineMappingDetailResult>> GetMeritDemeritDisciplineMappingDetail(string id);
        
        [Post("/school/merit-demerit/discipline-mapping-copy")]
        Task<ApiErrorResult<AddMeritDemeritDisciplineMappingCopyResult>> AddMeritDemeritDisciplineMappingCopy([Body] AddMeritDemeritDisciplineMappingCopyRequest body);

        [Get("/school/merit-demerit/discipline-mapping-level")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetMeritDemeritDisciplineMappingLevel(GetMeritDemeritLevelInfractionRequest query);

        [Get("/school/merit-demerit/discipline-mapping-check-point")]
        Task<ApiErrorResult<GetMeritDemeritDisciplineMappingCheckPointResult>> GetMeritDemeritDisciplineMappingCheckPoint(GetMeritDemeritComponentSettingRequest getMeritDemeritComponentSettingRequest);
        #endregion

        #region level-infraction
        [Post("/school/merit-demerit/level-infraction")]
        Task<ApiErrorResult> AddMeritDemeritLevelInfraction([Body] AddMeritDemeritLevelInfractionRequest body);

        [Get("/school/merit-demerit/level-infraction")]
        Task<ApiErrorResult<IEnumerable<GetMeritDemeritLevelInfractionResult>>> GetMeritDemeritLevelInfraction(GetMeritDemeritLevelInfractionRequest query);

        [Delete("/school/merit-demerit/level-infraction")]
        Task<ApiErrorResult> DeleteMeritDemeritLevelInfraction([Body] IEnumerable<string> body);
        #endregion

        #region Sanction-mapping
        [Post("/school/merit-demerit/Sanction-mapping")]
        Task<ApiErrorResult> AddMeritDemeritSanctionMapping([Body] AddMeritDemeritSanctionMappingRequest body);

        [Get("/school/merit-demerit/Sanction-mapping")]
        Task<ApiErrorResult<IEnumerable<GetMeritDemeritSanctionMappingResult>>> GetMeritDemeritSanctionMapping(GetMeritDemeritSanctionMappingRequest query);

        [Put("/school/merit-demerit/Sanction-mapping")]
        Task<ApiErrorResult> UpdateMeritDemeritSanctionMapping([Body] UpdateMeritDemeritSanctionMappingRequest body);

        [Delete("/school/merit-demerit/Sanction-mapping")]
        Task<ApiErrorResult> DeleteMeritDemeritSanctionMapping([Body] IEnumerable<string> body);

        [Get("/school/merit-demerit/Sanction-mapping/{id}")]
        Task<ApiErrorResult<GetMeritDemeritSanctionMappingDetailResult>> GetMeritDemeritSanctionMappingDetail(string id);

        [Post("/school/merit-demerit/Sanction-mapping-copy")]
        Task<ApiErrorResult<AddMeritDemeritSanctionMappingCopyResult>> AddMeritDemeritSanctionMappingCopy([Body] AddMeritDemeritSanctionMappingCopyRequest body);

        #endregion

        #region Score-continuation
        [Post("/school/merit-demerit/score-continuation")]
        Task<ApiErrorResult> UpdateScoreContinuationSetting([Body] UpdateScoreContinuationSettingRequest body);

        [Get("/school/merit-demerit/score-continuation")]
        Task<ApiErrorResult<IEnumerable<GetScoreContinuationSettingResult>>> GetScoreContinuationSetting(GetScoreContinuationSettingRequest query);
        #endregion
    }
}
