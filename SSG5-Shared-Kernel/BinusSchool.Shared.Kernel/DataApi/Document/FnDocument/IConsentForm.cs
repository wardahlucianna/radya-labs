using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Document.FnDocument.ConsentForm;
using Refit;

namespace BinusSchool.Data.Api.Document.FnDocument
{
    public interface IConsentForm : IFnDocument
    {
        [Get("/consent-form/check-hard-copy-document-submission")]
        Task<ApiErrorResult<CheckHardCopyDocumentSubmissionResult>> CheckHardCopyDocumentSubmission(CheckHardCopyDocumentSubmissionRequest param);
        
        [Get("/consent-form/entry-period")]
        Task<ApiErrorResult<IEnumerable<GetPeriodEntryResult>>> GetPeriodEntry(GetPeriodEntryRequest param);

        [Get("/consent-form/all-student-status")]
        Task<ApiErrorResult<IEnumerable<GetStudentConsentFormResult>>> GetStudentConsentForm(GetStudentConsentFormRequest param);

        [Get("/consent-form/get-consent-form-doc")]
        Task<ApiErrorResult<GetDocumentConsentFormResult>> GetDocumentConsentForm(GetDocumentConsentFormRequest param);

    }
}
