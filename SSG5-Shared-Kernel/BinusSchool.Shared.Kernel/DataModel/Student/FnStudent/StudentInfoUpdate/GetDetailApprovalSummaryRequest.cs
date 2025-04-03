using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentInfoUpdate
{
    public class GetDetailApprovalSummaryRequest
    {
        public string IdGrade { get; set; }
        public string IdClass { get; set; }
        public string Search { get; set; }
        public int IsParentUpdate { get; set; }
    }
}
