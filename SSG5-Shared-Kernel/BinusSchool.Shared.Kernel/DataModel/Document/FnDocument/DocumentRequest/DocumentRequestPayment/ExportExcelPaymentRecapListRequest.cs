using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class ExportExcelPaymentRecapListRequest
    {
        public string IdSchool { get; set; }
        public DateTime PaymentPeriodStartDate { get; set; }
        public DateTime PaymentPeriodEndDate { get; set; }
    }
}
