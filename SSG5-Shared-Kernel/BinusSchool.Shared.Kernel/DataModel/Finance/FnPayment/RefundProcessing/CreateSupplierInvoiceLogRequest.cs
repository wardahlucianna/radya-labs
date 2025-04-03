using System;
using System.Collections.Generic;
using System.Text;
using NPOI.OpenXmlFormats.Spreadsheet;
using Org.BouncyCastle.Asn1.Ocsp;

namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class CreateSupplierInvoiceLogRequest
    {
        public string IdRefundPayment { get; set; }
        public string IdBinusian { get; set; }
        public string MessageStatus { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
    }
}
