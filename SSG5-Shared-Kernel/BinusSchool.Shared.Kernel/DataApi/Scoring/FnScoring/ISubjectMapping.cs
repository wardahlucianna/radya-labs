using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.SubjectMapping;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ISubjectMapping : IFnScoring
    {
        [Get("/subjectmapping/mapping")]
        Task<ApiErrorResult<IEnumerable<GetSubjectMappingResult>>> GetSubjectMappings(GetSubjectMappingRequest query);

        [Get("/subjectmapping/mapping/{id}")]
        Task<ApiErrorResult<GetSubjectMappingByIdResult>> GetSubjectMappingById(string id);

        [Post("/subjectmapping/mapping")]
        Task<ApiErrorResult> SaveSubjectMapping([Body] SaveSubjectMappingRequest query);

        [Delete("/subjectmapping/mapping")]
        Task<ApiErrorResult> DeleteSubjectMapping([Body] IEnumerable<string> ids);

        [Post("/subjectmapping/transfer-subject-mapping")]
        Task<ApiErrorResult> TransferSubjectMapping([Body] TransferSubjectMappingRequest body);

        [Get("/subjectmapping/get-list-subject-mapping-detail-target")]
        Task<ApiErrorResult<GetListSubjectMappingDetailTargetResult>> GetListSubjectMappingDetailTarget(GetListSubjectMappingDetailTargetRequest query);

        [Get("/subjectmapping/get-list-subject-mapping-detail-source")]
        Task<ApiErrorResult<GetListSubjectMappingDetailSourceResult>> GetListSubjectMappingDetailSource(GetListSubjectMappingDetailSourceRequest query);

        [Post("/subjectmapping/save-subject-mapping-detail")]
        Task<ApiErrorResult> SaveSubjectMappingDetail([Body] SaveSubjectMappingDetailRequest query);

        [Get("/subjectmapping/get-subject-mapping-detail")]
        Task<ApiErrorResult<IEnumerable<GetSubjectMappingDetailResult>>> GetSubjectMappingDetail(GetSubjectMappingDetailRequest query);

        [Delete("/subjectmapping/delete-subject-mapping-detail")]
        Task<ApiErrorResult<DeleteSubjectMappingDetailResult>> DeleteSubjectMappingDetail([Body]List<DeleteSubjectMappingDetailRequest> query);

        [Get("/subjectmapping/get-subject-mapping-student-score")]
        Task<ApiErrorResult<IEnumerable<GetSubjectMappingStudentScoreResult>>> GetSubjectMappingStudentScore(GetSubjectMappingStudentScoreRequest query);

        [Post("/subjectmapping/sync-subject-mapping-score")]
        Task<ApiErrorResult> SyncSubjectMappingScore([Body] SyncSubjectMappingScoreRequest body);

        [Get("/subjectmapping/mapping-v2")]
        Task<ApiErrorResult<IEnumerable<GetSubjectMappingV2Result>>> GetSubjectMappingsV2(GetSubjectMappingV2Request query);

        [Post("/subjectmapping/mapping-v2")]
        Task<ApiErrorResult> SaveSubjectMappingV2([Body] SaveSubjectMappingV2Request body);

        [Get("/subjectmapping/mapping-id-v2")]
        Task<ApiErrorResult<GetSubjectMappingByIdV2Result>> GetSubjectMappingByIdV2(GetSubjectMappingByIdV2Request query);

        [Post("/subjectmapping/save-subject-mapping-detail-v2")]
        Task<ApiErrorResult> SaveSubjectMappingDetailV2([Body] SaveSubjectMappingDetailV2Request query);

        [Get("/subjectmapping/get-list-subject-mapping-detail-target-v2")]
        Task<ApiErrorResult<GetListSubjectMappingDetailTargetV2Result>> GetListSubjectMappingDetailTargetV2(GetListSubjectMappingDetailTargetV2Request query);

        [Get("/subjectmapping/get-list-subject-mapping-detail-source-v2")]
        Task<ApiErrorResult<GetListSubjectMappingDetailSourceV2Result>> GetListSubjectMappingDetailSourceV2(GetListSubjectMappingDetailSourceV2Request query);

        [Get("/subjectmapping/get-subject-mapping-detail-V2")]
        Task<ApiErrorResult<IEnumerable<GetSubjectMappingDetailV2Result>>> GetSubjectMappingDetailV2(GetSubjectMappingDetailV2Request query);

        [Delete("/subjectmapping/delete-subject-mapping-detail-v2")]
        Task<ApiErrorResult<DeleteSubjectMappingDetailV2Result>> DeleteSubjectMappingDetailV2([Body] List<DeleteSubjectMappingDetailV2Request> query);

        [Post("/subjectmapping/copy-mapping")]
        Task<ApiErrorResult> CopySubjectMapping([Body] CopySubjectMappingDetailRequest body);


        [Post("/subjectmapping/sync-subject-mapping-score-by-grade")]
        Task<ApiErrorResult> SyncSubjectMappingScoreByGrade([Body] SyncSubjectMappingScoreByGradeRequest body);
    }
}
