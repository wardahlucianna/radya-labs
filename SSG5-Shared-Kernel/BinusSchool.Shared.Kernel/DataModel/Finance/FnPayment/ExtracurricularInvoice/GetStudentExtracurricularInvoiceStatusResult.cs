using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice
{
    public class GetStudentExtracurricularInvoiceStatusResult
    {
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public List<GetStudentExtracurricularInvoiceStatusResult_Extracurricular> ExtracurricularList { get; set; }
    }
    public class GetStudentExtracurricularInvoiceStatusResult_Extracurricular
    {
        public string IdTransaction { get; set; }
        public NameValueVm Extracurricular { get; set; }
        public decimal ExtracurricularPrice { get; set; }
        public string VANumber { get; set; }
        public DateTime DueDatePayment { get; set; }
        public bool PaymentStatus { get; set; }
    }
}
