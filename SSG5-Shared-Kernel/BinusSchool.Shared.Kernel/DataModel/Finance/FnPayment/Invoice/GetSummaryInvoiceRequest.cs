using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.Invoice
{
    public class GetSummaryInvoiceRequest : CollectionRequest
    {
        public GetSummaryInvoiceRequest()
        {
            PackagePaymentName = "";
        }
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public string IdPackagePayment { get; set; }
        public string PackagePaymentName { get; set; }
        public string IdStatusWorkflowPayment { get; set; }
        
    }
}
