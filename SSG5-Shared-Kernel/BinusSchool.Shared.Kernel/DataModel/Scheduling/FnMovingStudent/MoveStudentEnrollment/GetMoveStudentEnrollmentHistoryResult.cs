using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment
{
    public class GetMoveStudentEnrollmentHistoryResult : CodeWithIdVm
    {
        public string newSubjectName { get; set; }
        public string newSubjectLevel { get; set; }
        public string oldSubjectName { get; set; }
        public string oldSubjectLevel { get; set; }
        public DateTime? effectiveDate { get; set; }
        public string note { get; set; }

    }
}
