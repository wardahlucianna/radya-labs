using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IDocumentRequestApprover : IFnDocument
    {
        [Get("/document-request-approver/get-approver-list-by-school")]
        Task<ApiErrorResult<IEnumerable<GetApproverListBySchoolResult>>> GetApproverListBySchool(GetApproverListBySchoolRequest param);

        [Get("/document-request-approver/get-unmapped-approver-staff-list")]
        Task<ApiErrorResult<IEnumerable<GetUnmappedApproverStaffListResult>>> GetUnmappedApproverStaffList(GetUnmappedApproverStaffListRequest param);

        [Post("/document-request-approver/add-document-request-approver")]
        Task<ApiErrorResult> AddDocumentRequestApprover([Body]AddDocumentRequestApproverRequest param);

        [Delete("/document-request-approver/remove-document-request-approver")]
        Task<ApiErrorResult> RemoveDocumentRequestApprover([Body] RemoveDocumentRequestApproverRequest param);

        [Get("/document-request-approver/check-admin-access-by-idbinusian")]
        Task<ApiErrorResult<CheckAdminAccessByIdBinusianResult>> CheckAdminAccessByIdBinusian(CheckAdminAccessByIdBinusianRequest param);
    }
}
