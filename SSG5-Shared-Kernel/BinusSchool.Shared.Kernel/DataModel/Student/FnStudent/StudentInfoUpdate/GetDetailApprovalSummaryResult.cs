using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentInfoUpdate
{
    public class GetDetailApprovalSummaryResult
    {
        public string Grade { get; set; }
        public string Class { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public DateTime? RequestDate { get; set; }
        public int RequestApproval { get; set; }
    }
}
