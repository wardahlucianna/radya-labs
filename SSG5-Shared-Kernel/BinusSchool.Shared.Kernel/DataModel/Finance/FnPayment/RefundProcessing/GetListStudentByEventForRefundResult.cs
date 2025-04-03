using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class GetListStudentByEventForRefundResult
    {
        public NameValueVm Student { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public bool BankAccountReady { get; set; }
        public string AccountNumber { get; set; }
        public ItemValueVm Bank { get; set; }
        public string AccountName { get; set; }
    }
}
