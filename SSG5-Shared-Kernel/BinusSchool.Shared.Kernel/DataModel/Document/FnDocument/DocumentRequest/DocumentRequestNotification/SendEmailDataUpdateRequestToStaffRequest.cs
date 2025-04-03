using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification
{
    public class SendEmailDataUpdateRequestToStaffRequest
    {
        public string IdSchool { get; set; }
        public string IdDocumentReqApplicant { get; set; }
        public string IdStudent { get; set; }
        public UpdateDocumentRequestStaffResult_Change ChangeData { get; set; }
    }
}
