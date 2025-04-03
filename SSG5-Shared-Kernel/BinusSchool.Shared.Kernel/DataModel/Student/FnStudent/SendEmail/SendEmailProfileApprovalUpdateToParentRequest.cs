using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.SendEmail
{
    public class SendEmailProfileApprovalUpdateToParentRequest
    {
        public string IdUser { get; set; }
        public List<string> IdStudentInfoUpdateList { get; set; }
    }
}
