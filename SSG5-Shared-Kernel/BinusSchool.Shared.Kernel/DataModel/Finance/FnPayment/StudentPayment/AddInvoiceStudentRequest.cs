using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class AddInvoiceStudentRequest
    {
        public string idEventPayment { get; set; }
        public List<InvoiceStudentVm> studentList { get; set; }
    }

    public class InvoiceStudentVm
    {
        public string idTransaction { get; set; }        
        public string idStudent { get; set; }
        public string idHomeroomStudent { get; set; }
        public decimal amount { get; set; }
        public bool isSendNotification { get; set; }
    }
}
