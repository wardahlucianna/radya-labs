using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.StudentBlocking;
using Refit;

namespace BinusSchool.Data.Api.User.FnBlocking.UserBlocking
{
    public interface IStudentBlocking : IFnBlocking
    {
        [Get("/user-blocking/student-blocking-download-excel")]
        Task<HttpResponseMessage> DownloadDocumentStudentBlocking(DownloadExcelStudentBlockingRequest body);

        [Multipart]
        [Post("/user-blocking/student-blocking-validation-excel-column-name")]
        Task<ApiErrorResult<object>> UploadExcelColumnNameStudentBlockingValidation(StreamPart file);

        [Multipart]
        [Post("/user-blocking/student-blocking-validation-excel-content-data")]
        Task<ApiErrorResult<object>> UploadExcelContentDataStudentBlockingValidation([Query] string idSchool, StreamPart file);

        [Get("/user-blocking/student-blocking-column-name")]
        Task<ApiErrorResult<GetColumnNameStudentBlockingResult>> GetColumnNameStudentBlocking(GetColumnNameStudentBlockingRequest query);

        [Get("/user-blocking/student-blocking-content-data")]
        Task<ApiErrorResult<IEnumerable<object>>> GetContentDataStudentBlocking(GetContentDataStudentBlockingRequest query);

        [Get("/user-blocking/student-blocking-access-block")]
        Task<ApiErrorResult<GetAccessBlockStudentBlockingResult>> GetAccessBlockStudentBlocking(GetAccessBlockStudentBlockingRequest query);
        
        [Post("/user-blocking/student-blocking")]
        Task<ApiErrorResult> AddStudentBlocking([Body] AddStudentBlockingRequest body);

        [Put("/user-blocking/student-blocking")]
        Task<ApiErrorResult> UpdateStudentBlocking([Body] UpdateStudentBlockingRequest body);

        [Get("/user-blocking/student-blocking-category-type")]
        Task<ApiErrorResult<GetStudentBlockingCategoryTypeResult>> GetStudentBlockingCategoryType(GetStudentBlockingCategoryTypeRequest query);
        
        [Put("/user-blocking/student-unblocking")]
        Task<ApiErrorResult> UpdateStudentUnBlocking([Body] UpdateStudentUnBlockingRequest body);

        [Get("/user-blocking/student-blocking")]
        Task<ApiErrorResult<IEnumerable<GetDataStudentBlockingResult>>> GetDataStudentBlocking(GetDataStudentBlockingRequest query);

        [Get("/user-blocking/student-blocking-download-excel-category")]
        Task<HttpResponseMessage> DownloadDocumentStudentBlockingByCategory(DownloadExcelStudentBlockingByCategoryRequest body);
    }
}
