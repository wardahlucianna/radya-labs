using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.SendEmail
{
    /*
     * CAUTION:
     * PLEASE BE CAREFUL WHEN MODIFY THIS FUNCTION BECAUSE THIS FUNCTION IS USED BY VIRTUAL ACCOUNT APPLICATION TOO!
     */

    public class SendEmailBulkPaidNotificationToParentRequest
    {
        public List<SendEmailBulkPaidNotificationToParentRequest_PaymentReferenceStudent> PaymentReferenceStudentList { get; set; }
        public bool? IsResendEmail { get; set; }
        public bool? IsInputManual { get; set; }
    }

    public class SendEmailBulkPaidNotificationToParentRequest_PaymentReferenceStudent
    {
        public string PaymentReference { get; set; }
        public string IdStudent { get; set; }
    }

    public class SendEmailBulkPaidNotificationToParentRequest_PackagePayment
    {
        public string IdPackagePayment { get; set; }
        public string PackageName { get; set; }
        public string Alias { get; set; }
        public bool IsElective { get; set; }
    }

    public class SendEmailBulkPaidNotificationToParentRequest_NotificationTemplate
    {
        public string IdSchool { get; set; }
        public string IdNotificationTemplate { get; set; }
        public string Title { get; set; }
        public string Push { get; set; }
        public string Email { get; set; }
        public bool EmailIsHtml { get; set; }
    }
}
