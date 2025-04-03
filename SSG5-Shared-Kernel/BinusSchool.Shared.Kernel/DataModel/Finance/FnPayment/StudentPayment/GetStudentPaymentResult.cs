using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class GetStudentPaymentResult : CodeWithIdVm
    {
        public ItemValueVm AcademicYear {  get; set; }
        public int Semester {  get; set; }
        public NameValueVm Student {  get; set; }
        public ItemValueVm Level { get; set; }
        public ItemValueVm Grade { get; set; }
        public ItemValueVm Homeroom { get; set; }
        public ItemValueVm Invoice { get; set; }
        public string VirtualAccountNumber {  get; set; }
        public decimal InvoiceAmount {  get; set; }
        public string PaymentStatus { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
