using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice
{
    public class CreateStudentExtracurricularInvoiceRequest
    {
        public List<CreateStudentExtracurricularInvoiceRequest_ExtracurricularData> ExtracurricularList { get; set; }
        public string IdStudent { get; set; }
        public string IdHomeroomStudent { get; set; }
        public int Semester { get; set; }
        public DateTime InvoiceStartDate { get; set; }
        public DateTime InvoiceEndDate { get; set; }
        public bool SendEmailNotification { get; set; }
    }

    public class CreateStudentExtracurricularInvoiceRequest_ExtracurricularData
    {
        public string IdExtracurricular { get; set; }
        public decimal ExtracurricularPrice { get; set; }
        public string ExtracurricularType { get; set; }
    }
}
