using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class GetInvoiceStudentResult
    {
        public string IdTransaction { set; get; }
        public string InvoiceDesc { set; get; }
        public string IdStudent { set; get; }
        public string StudentName { set; get; }
        public string SchoolLevel { set; get; }
        public string YearLevel { set; get; }
        public string Homeroom { set; get; }
        public string IdHomeroomStudent { set; get; }
        public string NoVA { set; get; }
        public string PackagePayment { set; get; }
        public string TotalInvoice { set; get; }
        public DateTime? StartDate { set; get; }
        public DateTime? EndDate { set; get; }
        public string StatusInvoice { set; get; }
        public DateTime? PaymentDate { set; get; }
        public bool isSendNotification { set; get; }
        public bool IsCbEnable { get; set; }
        public bool CanResendInvoiceEmail { get; set; }

    }
}
