using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification
{
    public class SendEmailNeedPaymentVerificationRequestToApproverRequest
    {
        public string IdSchool { get; set; }
        public string IdDocumentReqApplicant { get; set; }
        public string IdStudent { get; set; }
    }
}
