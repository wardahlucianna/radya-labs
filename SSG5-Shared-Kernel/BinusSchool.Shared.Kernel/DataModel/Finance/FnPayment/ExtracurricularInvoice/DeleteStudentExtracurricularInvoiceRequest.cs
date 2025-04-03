using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice
{
    public class DeleteStudentExtracurricularInvoiceRequest
    {
        public string IdStudent { get; set; }
        public string IdExtracurricular { get; set; }
    }
}
