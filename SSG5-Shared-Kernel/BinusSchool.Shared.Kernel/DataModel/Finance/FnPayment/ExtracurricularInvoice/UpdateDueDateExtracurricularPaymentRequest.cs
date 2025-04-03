using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice
{
    public class UpdateDueDateExtracurricularPaymentRequest
    {
        public DateTime? NewStartDatePayment { get; set; }
        public DateTime? NewEndDatePayment { get; set; }
        public string IdExtracurricular { get; set; }
        public List<string> IdStudentList { get; set; }
    }
}
