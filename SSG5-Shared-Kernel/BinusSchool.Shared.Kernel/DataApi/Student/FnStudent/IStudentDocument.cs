using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentDocument;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentDocument : IFnStudent
    {
        [Post("/student/Document/GetDocumentType")]
        Task<ApiErrorResult<IEnumerable<GetDocumentTypeResult>>> GetAllDocumentType([Body] CollectionRequest body);

        [Get("/student/Document/GetAdditionalDocumentByStudentID/{id}")]
        Task<ApiErrorResult<IEnumerable<GetDocumentByStudentResult>>> GetAdditionalDocumentByStudentID(string id, AdditionalDocumentByStudentIDRequest body);

        [Get("/student/Document/GetAdmissionDocument/{id}")]
        Task<ApiErrorResult<IEnumerable<GetDocumentByStudentResult>>> GetAdmissionDocument(string id);

        [Multipart]
        [Put("/student/Document/UpdateStudentDocument")]
        Task<ApiErrorResult> UpdateStudentDocument(string IdStudentDocument,
                                                    string IdDocument,
                                                    string FileName,
                                                    decimal FileSize,
                                                    string IdVerificationStatus,
                                                    string Comment,
                                                    string IdDocumentStatus,
                                                    bool IsStudentView,
                                                    string UserUp,
                                                    [AliasAs("file")] StreamPart file);

        [Multipart]
        [Post("/student/Document/AddStudentDocument")]
        Task<ApiErrorResult> AddStudentDocument(string IdStudent,
                                                                string IdDocument,
                                                                string FileName,
                                                                decimal FileSize,
                                                                string IdVerificationStatus,
                                                                string Comment,
                                                                string IdDocumentStatus,
                                                                bool IsStudentView,
                                                                string UserIn,
                                                                [AliasAs("file")] StreamPart file);

        [Delete("/student/Document/DeleteStudentDocument")]
        Task<ApiErrorResult> DeleteStudentDocument([Body] IEnumerable<string> ids);

        [Multipart]
        [Post("/student/Document/GetStudentDocumentByStudentIDFile")]
        Task<ApiErrorResult> GetStudentDocumentByStudentIDFile(string fileName);
    }

}
