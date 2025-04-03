using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestNotification
{
    public class SendEmailCancelRequestToStaffResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class SendEmailCancelRequestToStaffResult_Recepient
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}
