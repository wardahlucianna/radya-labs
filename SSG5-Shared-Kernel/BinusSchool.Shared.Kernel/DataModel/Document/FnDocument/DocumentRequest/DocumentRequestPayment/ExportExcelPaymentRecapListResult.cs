using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class ExportExcelPaymentRecapListResult
    {
        public string IdDocumentReqApplicant { get; set; }
        public string RequestNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public NameValueVm RequestedBy { get; set; }
        public string CreatedBy { get; set; }
        public NameValueVm Student { get; set; }
        public ItemValueVm HomeroomWhenRequestWasMade { get; set; }
        public GetPaymentRecapListResult_StudentStatus StudentStatusWhenRequestWasCreated { get; set; }
        public DateTime? EstimationFinishDate { get; set; }
        public GetPaymentRecapListResult_StatusWorkflow LatestDocumentReqStatusWorkflow { get; set; }
        public decimal TotalAmountReal { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public List<GetPaymentRecapListResult_Document> DocumentList { get; set; }
        public GetPaymentRecapListResult_Payment Payment { get; set; }
    }

    public class ExportExcelPaymentRecapListResult_ParamDesc
    {
        public string SchoolName { get; set; }
        public string FilterPaymentStartDate { get; set; }
        public string FilterPaymentEndDate { get; set; }
    }
}
