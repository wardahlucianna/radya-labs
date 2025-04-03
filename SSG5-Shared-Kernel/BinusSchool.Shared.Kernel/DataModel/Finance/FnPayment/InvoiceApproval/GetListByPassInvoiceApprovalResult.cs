using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.InvoiceApproval
{
    public class GetListByPassInvoiceApprovalResult : CodeWithIdVm
    {
        public string IdAcademicYear { set; get; }
        public int Semester { get; set; }
        public string IdEventPayment { get; set; }
        public string PaymentName { get; set; }
        public decimal TotalInvoice { get; set; }
        public List<string> PIC { get; set; }
        public List<string> ApproverList { get; set; }
    }
}
