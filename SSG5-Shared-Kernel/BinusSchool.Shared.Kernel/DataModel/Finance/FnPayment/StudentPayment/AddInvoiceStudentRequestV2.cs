using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class AddInvoiceStudentRequestV2
    {
        public string IdEventPayment { get; set; }
        public List<AddQueueInvoiceStudentRequest_InvoiceStudentVm> InvoiceStudentList { get; set; }
    }

    public class AddQueueInvoiceStudentRequest_InvoiceStudentVm
    {
        public string IdTransaction { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroomStudent { get; set; }
        public decimal Amount { get; set; }
        public bool IsSendNotification { get; set; }
    }
}
