using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class AddInputManualStudentPaymentRequest
    {
        public string IdTransaction { set; get; }
        public DateTime PaymentDate { set; get; }
        public string IdPaymentMethod { set; get; }
        public string Notes { set; get; }
        public string IdUserAction { set; get; }
    }
}
