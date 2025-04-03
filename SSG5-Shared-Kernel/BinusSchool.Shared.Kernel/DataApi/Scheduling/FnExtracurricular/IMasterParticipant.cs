using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IMasterParticipant : IFnExtracurricular
    {
        [Post("/master-participant/get-master-participant")]
        Task<ApiErrorResult<IEnumerable<GetMasterParticipantResult>>> GetMasterParticipant([Body] GetMasterParticipantRequest body);

        [Post("/master-participant/get-master-participant-v2")]
        Task<ApiErrorResult<IEnumerable<GetMasterParticipantResultV2>>> GetMasterParticipantV2([Body] GetMasterParticipantRequestV2 body);

        [Post("/master-participant/get-student-participant-by-extracurricular")]
        Task<ApiErrorResult<IEnumerable<GetStudentParticipantByExtracurricularResult>>> GetStudentParticipantByExtracurricular([Body] GetStudentParticipantByExtracurricularRequest body);

        [Get("/master-participant/get-unselected-student-by-homeroom")]
        Task<ApiErrorResult<IEnumerable<GetUnselectedStudentByHomeroomResult>>> GetUnselectedStudentByHomeroom(GetUnselectedStudentByHomeroomRequest body);

        [Post("/master-participant/add-student-participant")]
        Task<ApiErrorResult> AddStudentParticipant([Body] List<AddStudentParticipantRequest> body);

        [Put("/master-participant/update-student-participant")]
        Task<ApiErrorResult> UpdateStudentParticipant([Body] UpdateStudentParticipantRequest body);

        [Delete("/master-participant/delete-student-participant")]
        Task<ApiErrorResult<IEnumerable<DeleteStudentParticipantResult>>> DeleteStudentParticipant([Body] List<DeleteStudentParticipantRequest> body);

        [Delete("/master-participant/delete-all-student-participant")]
        Task<ApiErrorResult<IEnumerable<DeleteAllStudentParticipantResult>>> DeleteAllStudentParticipant([Body] DeleteAllStudentParticipantRequest body);

        [Post("/master-participant/student-participant-excel")]
        Task<HttpResponseMessage> ExportExcelStudentParticipant([Body] ExportExcelStudentParticipantRequest body);

        [Post("/master-participant/edit-join-date")]
        Task<ApiErrorResult> EditJoinDate([Body] EditJoinDateStudentParticipantRequest body);

        [Multipart]
        [Post("/master-participant/add-student-participant-by-excel")]
        Task<ApiErrorResult<AddStudentParticipantByExcelResult>> AddStudentParticipantByExcel(
            string IdExtracurricular,
            bool SendEmail,
            [AliasAs("file")] StreamPart file);
    }
}
