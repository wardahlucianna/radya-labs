using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class UpdateDocumentRequestStaffResult
    {

    }

    public class UpdateDocumentRequestStaffResult_Change
    {
        public string IdDocumentReqApplicant { get; set; }
        public NameValueVm UpdatedBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public UpdateDocumentRequestStaffResult_ChangeDetail ChangeDetail { get; set; }
    }

    public class UpdateDocumentRequestStaffResult_ChangeDetail
    {
        public NameValueVm OldParentApplicant { get; set; }
        public NameValueVm NewParentApplicant { get; set; }
        public int OldEstimationFinishDays { get; set; }
        public int NewEstimationFinishDays { get; set; }
        public DateTime? OldEstimationFinishDate { get; set; }
        public DateTime? NewEstimationFinishDate { get; set; }
        public List<UpdateDocumentRequestStaffResult_ChangeDetail_DocumentRequest> DocumentRequestList { get; set; }
    }

    public class UpdateDocumentRequestStaffResult_ChangeDetail_DocumentRequest
    {
        public string IdDocumentReqApplicantDetail { get; set; }
        public ItemValueVm OldPeriodDocument { get; set; }
        public ItemValueVm NewPeriodDocument { get; set; }
        public List<NameValueVm> OldBinusianPICList { get; set; }
        public List<NameValueVm> NewBinusianPICList { get; set; }
        public List<UpdateDocumentRequestStaffResult_ChangeDetail_FormAnswer> AdditionalFieldsList { get; set; }
    }

    public class UpdateDocumentRequestStaffResult_ChangeDetail_FormAnswer
    {
        public string IdDocumentReqFormFieldAnswered { get; set; }
        public List<string> OldTextValueList { get; set; }
        public List<string> NewTextValueList { get; set; }
    }
}
