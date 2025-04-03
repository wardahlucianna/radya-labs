using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using Refit;

namespace BinusSchool.Data.Api.Student.FnGuidanceCounseling
{
    public interface IReportStudentToGc : IFnGuidanceCounseling
    {
        #region report-student-to-gc
        [Get("/guidance-counseling/report-student-to-gc")]
        Task<ApiErrorResult<IEnumerable<GetReportStudentToGcResult>>> GetReportStudentToGcByTeacher(GetReportStudentToGcRequest query);

        [Post("/guidance-counseling/report-student-to-gc")]
        Task<ApiErrorResult> AddReportStudentToGc([Body] AddReportStudentToGcRequest body);

        [Get("/guidance-counseling/report-student-to-gc/{id}")]
        Task<ApiErrorResult<GetDetailReportStudentToGcResult>> GetDetailReportStudentToGc(string id);

        [Put("/guidance-counseling/report-student-to-gc")]
        Task<ApiErrorResult> UpdateReportStudentToGc([Body] UpdateReportStudentToGcRequest body);

        [Delete("/guidance-counseling/report-student-to-gc")]
        Task<ApiErrorResult> DeleteReportStudentToGc([Body] DeleteReportStudentToGcRequest body);
        #endregion

        #region Student-gc-report
        [Get("/guidance-counseling/report-student-to-gc-by-counselor")]
        Task<ApiErrorResult<IEnumerable<GetReportStudentToGcByCounsolorResult>>> GetReportStudentToGcByCounsolor(GetReportStudentToGcRequest query);

        [Post("/guidance-counseling/report-student-to-gc-by-counselor")]
        Task<ApiErrorResult> ReadStudentToGc([Body] ReadStudentToGcRequest body);
        #endregion

        #region Summary-counseling
        [Get("/guidance-counseling/report-student-to-gc-by-student")]
        Task<ApiErrorResult<IEnumerable<GetReportStudentToGcByStudentResult>>> GetReportStudentToGcByStudent(GetReportStudentToGcByStudentRequest query);

        [Get("/guidance-counseling/report-student-to-gc-counseling-entry")]
        Task<ApiErrorResult<IEnumerable<GetCounselingServicesEntryByStudentResult>>> GetCounselingServicesEntryByStudent(GetCounselingServicesEntryByStudentRequest query);

        [Get("/guidance-counseling/report-student-to-gc-summary-counseling")]
        Task<ApiErrorResult<IEnumerable<GetSummaryCounselingResult>>> GetSummaryCounseling(GetSummaryCounselingRequest query);

        [Get("/guidance-counseling/report-student-to-gc-wizard-counseling")]
        Task<ApiErrorResult<IEnumerable<GetWizardCounselingResult>>> GetWizardCounseling(GetWizardCounselingRequest query);

        [Post("/guidance-counseling/report-student-to-gc-excel-counselor-services-entry")]
        Task<HttpResponseMessage> GetExcelCounselingServicesEntryByStudent([Body] GetExcelCounselingServicesEntryByStudentRequest body);

        [Post("/guidance-counseling/report-student-to-gc-excel-report-student")]
        Task<HttpResponseMessage> GetExcelReportStudent([Body] GetExcelReportStudentRequest body);

        [Get("/guidance-counseling/report-student-to-gc-access")]
        Task<ApiErrorResult<GetGcAccessResult>> GetGcAccess(GetGcAccessRequest query);
        #endregion
    }
}
