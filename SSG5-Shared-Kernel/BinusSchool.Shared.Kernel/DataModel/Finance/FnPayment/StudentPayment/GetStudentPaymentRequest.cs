using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class GetStudentPaymentRequest : CollectionRequest
    {
        public string IdStudent {  get; set; }
        public string IdSchool { get; set; }
    }
}
