using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Finance.FnPayment.StudentPayment
{
    public class GetStudentForImportEventRequest
    {
        public string IdEvent { set; get; }
        public string IdEventPayment { get; set; }
    }
}
