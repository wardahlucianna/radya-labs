using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.RefundProcessing
{
    public class CreateStudentRefundPaymentRequest
    {
        public string IdSchool { get; set; }
        public DateTime? RefundDate { get; set; }
        //public List<CreateStudentExtracurricularInvoiceRequest_ExtracurricularData> ExtracurricularList { get; set; }
        //public string IdRefundStudent { get; set; }
        //public string IdRefundPayment { get; set; }
        //public string IdStudent { get; set; }
        //public string IdHomeroomStudent { get; set; }
        //public int Semester { get; set; }
        //public DateTime ExpectedDate { get; set; }
        //public DateTime? RefundDate { get; set; }
        //public int? TotalRefund { get; set; }
        //public string AccountNumber { get; set; }
    }

    //public class CreateStudentExtracurricularInvoiceRequest_ExtracurricularData
    //{
    //    public string IdExtracurricular { get; set; }
    //    public decimal ExtracurricularPrice { get; set; }
    //}
}
