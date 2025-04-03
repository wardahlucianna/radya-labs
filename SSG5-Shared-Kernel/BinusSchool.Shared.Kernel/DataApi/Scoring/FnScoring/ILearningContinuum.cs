using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumEntry;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumSummary;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ILearningContinuum : IFnScoring
    {
        [Get("/learningcontinuum/get-learning-continuum")]
        Task<ApiErrorResult<GetLearningContinuumResult>> GetLearningContinuum(GetLearningContinuumRequest query);

        [Get("/learningcontinuum/get-list-student-learning-continuum-entry")]
        Task<ApiErrorResult<IEnumerable<GetListStudentLearningContinuumEntryResult>>> GetListStudentLearningContinuumEntry(GetListStudentLearningContinuumEntryRequest query);

        [Get("/learningcontinuum/get-list-subject-continuum-by-student")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetListSubjectContinuumByStudent(GetListSubjectContinuumByStudentRequest query);

        [Get("/learningcontinuum/get-list-loc-item")]
        Task<ApiErrorResult<GetLOCItemResult>> GetLOCItem(GetLOCItemRequest query);

        [Get("/learningcontinuum/get-learning-continuum-detail-by-student")]
        Task<ApiErrorResult<GetLearningContinuumEntryDetailByStudentResult>> GetLearningContinuumEntryDetailByStudent(GetLearningContinuumEntryDetailByStudentRequest query);

        [Post("/learningcontinuum/save-learning-continuum-entry-by-student")]
        Task<ApiErrorResult> SaveLearningContinuumEntryByStudent(SaveLearningContinuumEntryByStudentRequest query);

        [Get("/learningcontinuum/get-list-learning-continuum-history")]
        Task<ApiErrorResult<IEnumerable<GetListLearningContinuumHistoryResult>>> GetListLearningContinuumHistory(GetListLearningContinuumHistoryRequest query);

        [Get("/learningcontinuum/get-list-learning-continuum-summary")]
        Task<ApiErrorResult<IEnumerable<GetListLearningContinuumSummaryResult>>> GetListLearningContinuumSummary(GetListLearningContinuumSummaryRequest query);

        [Get("/learningcontinuum/get-learning-continuum-summary-detail-by-student")]
        Task<ApiErrorResult<GetLearningContinuumSummaryDetailByStudentResult>> GetLearningContinuumSummaryDetailByStudent(GetLearningContinuumSummaryDetailByStudentRequest query);

        [Get("/learningcontinuum/get-list-all-subject-continuum")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetListAllSubjectContinuum(GetListAllSubjectContinuumRequest query);

        [Post("/learningcontinuum/export-excel-learning-continuum-summary")]
        Task<HttpResponseMessage> ExportExcelLearningContinuumSummary([Body] ExportExcelLearningContinuumSummaryRequest query);
    }
}
