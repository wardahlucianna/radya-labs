using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.Invoice
{
    public class AddMasterInvoiceRequest
    {
        public bool invoiceType { set; get; }
        public string idSchool { set; get; }
        public string idAcademicYear { set; get; }
        public int semester { set; get; }       
        public string idEventType { set; get; }  
        public string idEvent { set; get; }      
        public string paymentName { set; get; }
        public string idPackagePayment { set; get; }
        public string description { set; get; }      
        public DateTime paymentStartDateTime { set; get; }
        public DateTime paymentEndDateTime { set; get; }
        public List<string> PIC { set; get; }
        public bool NeedApproval { set; get; }
        public List<ApproverInvoicePaymentVm> approver { set; get; }


    }
    public class ApproverInvoicePaymentVm
    {
        public string idBinusian { set; get; }
        public int levelApproval { set; get; }
    }
}
