using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class GetPaymentMethodBySchoolResult : ItemValueVm
    {
        public string IdDocumentReqPaymentMethod { get; set; }
        public string Name { get; set; }
        public bool UsingManualVerification { get; set; }
        public bool IsVirtualAccount { get; set; }
        public string AccountNumber { get; set; }
        public string DescriptionHTML { get; set; }
        public string ImageUrl { get; set; }
    }
}
