using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.Filter;
using BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus;
using BinusSchool.Data.Model.Scoring.FnScoring.ProgressStatus.Filter;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface IProgressStatus : IFnScoring
    {
        #region Filter
        [Post("/progressStatus/get-specific-grade-by-position-filter")]
        Task<ApiErrorResult<IEnumerable<GetSpecificGradeByPositionResult>>> GetSpecificGradeByPosition([Body] GetSpecificGradeByPositionRequest body);

        [Post("/progressStatus/get-specific-classroom-by-position-filter")]
        Task<ApiErrorResult<IEnumerable<GetSpecificClassroomByPositionResult>>> GetSpecificClassroomByPosition([Body] GetSpecificClassroomByPositionRequest body);

        [Post("/progressStatus/get-specific-students-by-position-filter")]
        Task<ApiErrorResult<IEnumerable<GetSpecificStudentsByPositionResult>>> GetSpecificStudentsByPosition([Body] GetSpecificStudentsByPositionRequest body);
        #endregion

        [Get("/progressStatus/students-by-filter")]
        Task<ApiErrorResult<IEnumerable<GetStudentsByFilterProgressStatusResult>>> GetStudentsByFilterProgressStatus(GetStudentsByFilterProgressStatusRequest query);

        [Get("/progressStatus/widget-counter")]
        Task<ApiErrorResult<GetWidgetCounterResult>> GetWidgetCounter(GetWidgetCounterRequest query);

        [Get("/progressStatus/progress-student-status-mapping")]
        Task<ApiErrorResult<IEnumerable<GetProgressStudentStatusMappingResult>>> GetProgressStudentStatusMapping(GetProgressStudentStatusMappingRequest query);

        [Get("/progressStatus/progress-status")]
        Task<ApiErrorResult<GetProgressStatusResult>> GetProgressStatus(GetProgressStatusRequest query);

        [Get("/progressStatus/progress-status-spesific-student")]
        Task<ApiErrorResult<GetProgressStatusSpesificStudentResult>> GetProgressStatusSpesificStudent(GetProgressStatusSpesificStudentRequest query);

        [Get("/progressStatus/progress-status-byhomeroom")]
        Task<ApiErrorResult<List<GetProgressStatusHomeroomResult>>> GetProgressStatusByHomeroom(GetProgressStatusHomeroomRequest query);

        [Post("/progressStatus/entry-progress-status")]
        Task<ApiErrorResult> EntryProgressStatus([Body] EntryProgressStatusRequest query);

        [Put("/progressStatus/approval-progress-status")]
        Task<ApiErrorResult> ApprovalProgressStatus([Body] ApprovalProgressStatusRequest query);

        [Get("/progressStatus/student-progress-status-studentparentview")]
        Task<ApiErrorResult<GetStudentProgressStatusbyAcadYearResult>> GetStudentProgressStatusbyAcadYear(GetStudentProgressStatusbyAcadYearRequest query);
    }
}
