using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface ICreativityActivityService : IFnStudent
    {
        #region Experience
        [Get("/student/creativity-activity-service/get-list-experience")]
        Task<ApiErrorResult<IEnumerable<GetListExperienceResult>>> GetListExperience(GetListExperienceRequest request);

        [Get("/student/creativity-activity-service/get-timeline")]
        Task<ApiErrorResult<GetTimelineBySupervisorResult>> GetTimeline(GetTimelineRequest request);
        
        [Post("/student/creativity-activity-service/add-experience")]
        Task<ApiErrorResult> AddExperience(AddExperienceRequest request);

        [Put("/student/creativity-activity-service/update-experience")]
        Task<ApiErrorResult> UpdateExperience(UpdateExperienceRequest request);

        [Get("/student/creativity-activity-service/detail-experience")]
        Task<ApiErrorResult<DetailExperienceResult>> DetailExperience(DetailExperienceRequest query);
        
        [Put("/student/creativity-activity-service/update-status-experience")]
        Task<ApiErrorResult> UpdateStatusExperience(UpdateStatusExperienceRequest request);

        [Put("/student/creativity-activity-service/update-overall-progress-status-student")]
        Task<ApiErrorResult> UpdateOverallProgressStatusStudent(UpdateOverallProgressStatusStudentRequest request);

        [Delete("/student/creativity-activity-service/delete-experience")]
        Task<ApiErrorResult> DeleteExperience([Body] DeleteExperienceRequest body);
        #endregion

        #region Learning Outcome
        [Get("/student/creativity-activity-service/get-list-learning-outcome")]
        Task<ApiErrorResult<IEnumerable<GetListLearningOutcomeResult>>> GetListLearningOutcome(GetListLearningOutcomeRequest request);
        #endregion

        #region Experience Supervisor
        [Get("/student/creativity-activity-service/get-ay-grade-by-supervisor")]
        Task<ApiErrorResult<IEnumerable<GetAcademicYearAndGradeBySupervisorResult>>> GetAcademicYearAndGradeBySupervisor(GetAcademicYearAndGradeBySupervisorRequest request);

        [Get("/student/creativity-activity-service/get-list-experience-by-supervisor")]
        Task<ApiErrorResult<IEnumerable<GetListExperienceBySupervisorResult>>> GetListExperienceBySupervisor(GetListExperienceBySupervisorRequest body);

        [Get("/student/creativity-activity-service/get-list-student-by-supervisor")]
        Task<ApiErrorResult<IEnumerable<GetListStudentBySupervisorResult>>> GetListStudentBySupervisor(GetListStudentBySupervisorRequest request);

        [Get("/student/creativity-activity-service/get-timeline-by-supervisor")]
        Task<ApiErrorResult<GetTimelineBySupervisorResult>> GetTimelineBySupervisor(GetTimelineBySupervisorRequest body);
        #endregion

        #region Student Information
        [Get("/student/creativity-activity-service/get-list-ay-for-student-experience")]
        Task<ApiErrorResult<IEnumerable<GetAcademicYearForStudentExperienceResult>>> GetAcademicYearForStudentExperience(GetAcademicYearForStudentExperienceRequest request);
        
        [Get("/student/creativity-activity-service/get-student-information-by-ay")]
        Task<ApiErrorResult<GetStudentInformationByAcademicYearResult>> GetStudentInformationByAcademicYear(GetStudentInformationByAcademicYearRequest query);
        #endregion

        #region Evidences
        [Get("/student/creativity-activity-service/get-list-evidences")]
        Task<ApiErrorResult<IEnumerable<GetListEvidencesResult>>> GetListEvidences(GetListEvidencesRequest request);
        [Get("/student/creativity-activity-service/detail-evidences")]
        Task<ApiErrorResult<DetailEvidencesResult>> DetailEvidences(DetailEvidencesRequest query);

        [Post("/student/creativity-activity-service/add-evidences")]
        Task<ApiErrorResult> AddEvidences(AddEvidencesRequest request);
        [Put("/student/creativity-activity-service/update-evidences")]
        Task<ApiErrorResult> UpdateEvidences(UpdateEvidencesRequest request);

        [Delete("/student/creativity-activity-service/delete-evidences")]
        Task<ApiErrorResult> DeleteEvidences([Body] DeleteEvidencesRequest body);
        #endregion

        #region Download
        [Get("/student/creativity-activity-service/get-timeline-to-pdf")]
        Task<ApiErrorResult<GetTimelineBySupervisorResult>> GetTimelineToPdf(GetTimelineToPdfRequest query);

        [Get("/student/creativity-activity-service/get-experience-to-pdf")]
        Task<ApiErrorResult<IEnumerable<GetExperienceResult>>> GetExperienceToPdf(GetExperienceToPdfRequest query);

        [Post("/student/creativity-activity-service/cas-request-download")]
        Task<ApiErrorResult> CasRequestDownload([Body]CasExperienceDownloadRequest body);

        [Post("/student/creativity-activity-service/email-download")]
        Task<ApiErrorResult> EmailDownload([Body] EmailDownloadResult body);
        #endregion

        #region Comment Evidences
        [Get("/student/creativity-activity-service/get-list-comment-evidences")]
        Task<ApiErrorResult<IEnumerable<GetListCommentEvidencesResult>>> GetListCommentEvidences(GetListCommentEvidencesRequest request);

        [Post("/student/creativity-activity-service/save-comment-evidences")]
        Task<ApiErrorResult> SaveCommentEvidences(SaveCommentEvidencesRequest request);

        [Delete("/student/creativity-activity-service/delete-comment-evidences")]
        Task<ApiErrorResult> DeleteCommentEvidences([Body] DeleteCommentEvidencesRequest body);

        #endregion

        #region CAS
        [Get("/student/creativity-activity-service/get-list-student-by-cas")]
        Task<ApiErrorResult<IEnumerable<GetListStudentByCASResult>>> GetListStudentByCAS(GetListStudentByCASRequest request);
        #endregion

    }
}
