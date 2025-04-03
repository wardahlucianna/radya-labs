using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class GetListDetailStudentRefundProcessingResult
    {
        public ItemValueVm RefundPayment { get; set; }
        public NameValueVm Student { get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public DateTime? TransferDate { get; set; }
        public string SchoolName { get; set; }
        public bool BankAccountReady { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public string BankAccount { get; set; }
        public ItemValueVm Bank { get; set; }
        public int? Amount { get; set; }
        public bool IsGenerate { get; set; }
        public bool IsSendEmail { get; set; }
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
    }
}
