using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ReportIssueDate;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ScoreViewMapping;
using BinusSchool.Data.Model.Scoring.FnScoring.ReportSetting.ViewTemplate;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IReportSetting : IFnScoring
    {
        #region ScoreViewTemplate
        [Delete("/BNS/ReportSetting/DeleteScoreViewTemplate")]
        Task<ApiErrorResult> DeleteScoreViewTemplate([Body] IEnumerable<string> ids);

        [Get("/BNS/ReportSetting/ScoreViewTemplate/{id}")]
        Task<ApiErrorResult<GetScoreViewTemplateDetailResult>> GetScoreViewTemplateDetail(string id);

        [Get("/BNS/ReportSetting/ScoreViewTemplate")]
        Task<ApiErrorResult<IEnumerable<GetScoreViewTemplateResult>>> GetScoreViewTemplate(GetScoreViewTemplateRequest query);
             
        [Post("/BNS/ReportSetting/AddScoreViewTemplate")]
        Task<ApiErrorResult> AddScoreViewTemplate([Body] AddScoreViewTemplateRequest query);
        
        [Put("/BNS/ReportSetting/UpdateScoreViewTemplate")]
        Task<ApiErrorResult> UpdateScoreViewTemplate([Body] UpdateScoreViewTemplateRequest query);

        #endregion

        #region ScoreViewMapping
        [Get("/BNS/ReportSetting/ScoreViewMappingByViewTemplate")]
        Task<ApiErrorResult<GetSoreViewMappingByViewTemplateResult>> GetScoreViewMappingByViewTemplate(GetSoreViewMappingByViewTemplateRequest query);

        [Post("/BNS/ReportSetting/UpdateScoreViewMapping")]
        Task<ApiErrorResult> UpdateScoreViewMapping([Body] UpdateScoreViewMappingRequest query);

        [Get("/BNS/ReportSetting/GetScoreViewMapping")]
        Task<ApiErrorResult<IEnumerable<GetScoreViewMappingResult>>> GetScoreViewMapping(GetScoreViewMappingRequest query);

        [Delete("/BNS/ReportSetting/DeleteScoreViewMapping")]
        Task<ApiErrorResult> DeleteScoreViewMapping([Body] IEnumerable<string> ids);

        [Post("/BNS/ReportSetting/CopyScoreViewMapping")]
        Task<ApiErrorResult<CopyScoreViewMappingResult>> CopyScoreViewMapping([Body] CopyScoreViewMappingRequest query);

        #endregion

        #region ReportIssueDate
        [Get("/BNS/ReportSetting/GetReportIssueDateList")]
        Task<ApiErrorResult<IEnumerable<GetReportIssueDateListResult>>> GetReportIssueDateList(GetReportIssueDateListRequest query);

        [Post("/BNS/ReportSetting/AddReportIssueDate")]
        Task<ApiErrorResult> AddReportIssueDate([Body] AddReportIssueDateRequest query);

        [Put("/BNS/ReportSetting/UpdateReportIssueDate")]
        Task<ApiErrorResult> UpdateReportIssueDate([Body] UpdateReportIssueDateRequest query);

        [Delete("/BNS/ReportSetting/DeleteReportIssueDate")]
        Task<ApiErrorResult> DeleteReportIssueDate([Body] IEnumerable<string> ids);
        #endregion
    }
}
