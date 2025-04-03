using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice
{
    public class GetStudentExtracurricularInvoiceStatusRequest
    {
        public string IdAcademicYear { get; set; }
        public int? Semester { get; set; }
        public List<string> IdStudentList { get; set; }
    }
}
