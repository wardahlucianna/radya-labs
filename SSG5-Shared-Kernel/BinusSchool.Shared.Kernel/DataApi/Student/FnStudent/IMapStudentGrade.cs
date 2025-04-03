using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentGrade;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IMapStudentGrade : IFnStudent
    {
        [Get("/student/map-student-grade")]
        Task<ApiErrorResult<IEnumerable<GetMapStudentGradeResult>>> GetMapStudentGrades(GetMapStudentGradeRequest request);

        [Post("/student/map-student-grade")]
        Task<ApiErrorResult> CreateMapStudentGrades(CreateMapStudentGradeRequest body);
        [Put("/student/map-student-grade")]
        Task<ApiErrorResult> CopyNextAYMapStudentGrade(CopyNextAYMapStudentGradeRequest request);

        [Get("/student/map-student-grade/download-excel")]
        Task<HttpResponseMessage> GetDownloadTemplateMapStudentGrade();

        [Multipart]
        [Post("/student/map-student-grade/upload-excel")]
        Task<ApiErrorResult<UploadExcelMapStudentGradeResult>> UploadExcelMapStudentGrade(StreamPart file, [Query] UploadExcelMapStudentGradeRequest request);
    }
}
