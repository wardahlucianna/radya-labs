using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice
{
    public class GetStudentExtracurricularInvoiceStatusFormatDataTableResult
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public NameValueVm Extracurricular { get; set; }
        public decimal ExtracurricularPrice { get; set; }
        public string VANumber { get; set; }
        public DateTime DueDatePayment { get; set; }
        public bool PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
