using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.InvoiceApproval
{
    public class GetListByPassInvoiceApprovalRequest : CollectionRequest
    {
        public string IdAcademicYear { set; get; }
        public int? Semester { get; set; }
    }
}
