using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentStatus
{
    public class GetStudentStatusHistoryResult
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm StudentStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Remarks { get; set; }
    }
}
