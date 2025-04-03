using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using Refit;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IServiceAsAction : IFnStudent
    {
        [Get("/student/service-as-action/get-list-grade-teacher-privilege")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetListGradeTeacherPrivilege(GetListGradeTeacherPrivilegeRequest query);

        [Get("/student/service-as-action/get-list-student-service-as-action")]
        Task<ApiErrorResult<IEnumerable<GetListStudentServiceAsActionResult>>> GetListStudentServiceAsAction(GetListStudentServiceAsActionRequest query);

        [Get("/student/service-as-action/get-list-experience-per-student")]
        Task<ApiErrorResult<GetListExperiencePerStudentResult>> GetListExperiencePerStudent(GetListExperiencePerStudentRequest query);

        [Get("/student/service-as-action/get-list-learning-outcome-for-sac")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetListLearningOutcomeForSAC(GetListLearningOutcomeForSACRequest query);

        [Get("/student/service-as-action/get-service-as-action-detail-form")]
        Task<ApiErrorResult<GetServiceAsActionDetailFormResult>> GetServiceAsActionDetailForm(GetServiceAsActionDetailFormRequest query);

        [Get("/student/service-as-action/get-list-mapping-learning-outcome")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetListMappingLearningOutcome(GetListMappingLearningOutcomeRequest query);

        [Post("/student/service-as-action/save-experience-service-as-action")]
        Task<ApiErrorResult> SaveExperienceServiceAsAction([Body] SaveExperienceServiceAsActionRequest body);

        [Delete("/student/service-as-action/delete-experience-service-as-action")]
        Task<ApiErrorResult> DeleteExperienceServiceAsAction([Body] DeleteExperienceServiceAsActionRequest query);

        [Post("/student/service-as-action/save-service-as-action-status")]
        Task<ApiErrorResult> SaveServiceAsActionStatus([Body] SaveServiceAsActionStatusRequest body);

        [Post("/student/service-as-action/save-service-as-action-evidence")]
        Task<ApiErrorResult> SaveServiceAsActionEvidence([Body] SaveServiceAsActionEvidenceRequest body);

        [Delete("/student/service-as-action/delete-service-as-action-evidence")]
        Task<ApiErrorResult> DeleteServiceAsActionEvidence([Body] DeleteServiceAsActionEvidenceRequest query);

        [Post("/student/service-as-action/save-service-as-action-comment")]
        Task<ApiErrorResult> SaveServiceAsActionComment([Body] SaveServiceAsActionCommentRequest body);

        [Delete("/student/service-as-action/delete-service-as-action-comment")]
        Task<ApiErrorResult> DeleteServiceAsActionComment([Body] DeleteServiceAsActionCommentRequest query);

        [Post("/student/service-as-action/save-overall-status-experience")]
        Task<ApiErrorResult> SaveOverallStatusExperience([Body] SaveOverallStatusExperienceRequest body);
    }
}
