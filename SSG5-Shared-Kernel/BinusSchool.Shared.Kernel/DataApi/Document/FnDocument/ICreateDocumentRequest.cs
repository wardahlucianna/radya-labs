using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface ICreateDocumentRequest : IFnDocument
    {
        [Get("/create-document-request/get-student-parent-homeroom-information")]
        Task<ApiErrorResult<GetStudentParentHomeroomInformationResult>> GetStudentParentHomeroomInformation(GetStudentParentHomeroomInformationRequest param);

        [Get("/create-document-request/get-parent-with-role-by-student")]
        Task<ApiErrorResult<IEnumerable<GetParentWithRoleByStudentResult>>> GetParentWithRoleByStudent(GetParentWithRoleByStudentRequest param);

        [Get("/create-document-request/get-default-pic-list")]
        Task<ApiErrorResult<IEnumerable<GetDefaultPICListResult>>> GetDefaultPICList(GetDefaultPICListRequest param);

        [Get("/create-document-request/get-document-type-by-category")]
        Task<ApiErrorResult<IEnumerable<GetDocumentTypeByCategoryResult>>> GetDocumentTypeByCategory(GetDocumentTypeByCategoryRequest param);

        [Get("/create-document-request/get-student-ay-and-grade-history-list")]
        Task<ApiErrorResult<IEnumerable<GetStudentAYAndGradeHistoryListResult>>> GetStudentAYAndGradeHistoryList(GetStudentAYAndGradeHistoryListRequest param);

        [Get("/create-document-request/get-document-request-type-detail-configuration")]
        Task<ApiErrorResult<GetDocumentRequestTypeDetailAndConfigurationResult>> GetDocumentRequestTypeDetailAndConfiguration(GetDocumentRequestTypeDetailAndConfigurationRequest param);

        [Post("/create-document-request/create-document-request-by-staff")]
        Task<ApiErrorResult> CreateDocumentRequestStaff([Body]CreateDocumentRequestStaffRequest param);

        [Post("/create-document-request/create-document-request-by-parent")]
        Task<ApiErrorResult<CreateDocumentRequestParentResult>> CreateDocumentRequestParent([Body]CreateDocumentRequestParentRequest param);

        [Put("/create-document-request/update-document-request-by-staff")]
        Task<ApiErrorResult> UpdateDocumentRequestStaff([Body] UpdateDocumentRequestStaffRequest param);
    }
}
