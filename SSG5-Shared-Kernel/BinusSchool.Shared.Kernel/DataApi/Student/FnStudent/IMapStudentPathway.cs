using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IMapStudentPathway : IFnStudent
    {
        [Get("/student/map-student-pathway")]
        Task<ApiErrorResult<IEnumerable<GetMapStudentPathwayResult>>> GetMapStudentPathways(GetMapStudentPathwayRequest request);

        [Put("/student/map-student-pathway")]
        Task<ApiErrorResult> UpdateMapStudentPathway(UpdateMapStudentPathwayRequest body);

        [Get("/student/map-student-pathway/template")]
        Task<HttpResponseMessage> GetMapStudentPathwayTemplate(DownloadMapStudentPathwayRequest request);

        [Multipart]
        [Post("/student/map-student-pathway/import")]
        Task<ApiErrorResult<IEnumerable<GetMapStudentPathwayResult>>> ImportMapStudentPathway(
            StreamPart stream, 
            [Query]string IdSchool,
            [Query]string IdAcadyear 
            );

        #region copy next ay
        [Get("/student/map-student-pathway/copy")]
        Task<ApiErrorResult<IEnumerable<GetCopyMapStudentPathwayResult>>> GetCopyMapStudentPathway(GetMapStudentPathwayRequest request);

        [Put("/student/map-student-pathway/copy")]
        Task<ApiErrorResult> CopyMapStudentPathway(CopyMapStudentPathwayRequest body);
        #endregion
    }
}
