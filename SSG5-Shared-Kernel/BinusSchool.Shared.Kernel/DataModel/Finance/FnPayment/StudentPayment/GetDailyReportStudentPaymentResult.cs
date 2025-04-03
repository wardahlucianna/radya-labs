using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class GetDailyReportStudentPaymentResult
    {
        public string AcademicYear {  get; set; }
        public int Semester { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Category { get; set; }
        public string PaymentName {  get; set; }
        public string VirtualAccountNumber { get; set; }
        public decimal PaymentNominal { get; set; }
        public string StudentName { get; set; }
        public string Classroom { get; set; }
    }
}
