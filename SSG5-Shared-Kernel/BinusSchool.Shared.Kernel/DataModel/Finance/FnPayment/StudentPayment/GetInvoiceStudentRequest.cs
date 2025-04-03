using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class GetInvoiceStudentRequest : CollectionRequest
    {
        public string IdEventPayment { set; get; }
        public string IdExtracurricular { set; get; }
        public string IdStudent { set; get; }
        public string IdStatusWorkflowPayment { set; get; }
    }
}
