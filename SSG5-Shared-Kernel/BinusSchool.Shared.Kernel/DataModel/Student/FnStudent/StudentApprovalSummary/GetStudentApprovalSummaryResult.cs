using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentApprovalSummary
{
    public class GetStudentApprovalSummaryResult
    {
        public string GradeId { get; set; }
        public string Grade { get; set; }
        public string Class { get; set; }
        public string ClassID { get; set; }
        public int FromStaffDesk { get; set; }
        public int FromParentDesk { get; set; }
    }
}
