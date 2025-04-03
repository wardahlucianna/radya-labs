using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestPayment
{
    public class GetPaymentMethodBySchoolRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
        public string IdDocumentReqPaymentMethod { get; set; }
    }
}
