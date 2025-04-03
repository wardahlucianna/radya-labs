using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.Invoice
{
    public class GetMasterInvoiceDetailResult
    {
        public bool isEditable { set; get; }
        public bool invoiceType { set; get; }
        public ItemValueVm academicYear { set; get; }
        public ItemValueVm semester { set; get; }
        public ItemValueVm? eventType { set; get; }
        public ItemValueVm? eventName { set; get; }
        public ItemValueVm package { set; get; }
        public ItemValueVm paymentName { set; get; }
        public string description { set; get; }
        public DateTime paymentStartDateTime { get; set; }
        public DateTime paymentEndDateTime { get; set; }
        public List<ItemValueVm> PIC { get; set; }
        public List<ApproverInvoiceVm> approver { get; set; }
    }

    public class ApproverInvoiceVm  : ItemValueVm
    {
        public int level { set; get; }
    }
}
