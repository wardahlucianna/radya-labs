using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme
{
    public class EmailStudentProgrammeResult
    {
        public string studentId { get; set; }
        public string studentName { get; set; }
        public string homeroom { get; set; }
        public string newProgramme { get; set; }
        public string oldProgramme { get; set; }
        public string effectiveDate { get; set; }
        public List<string> idUserTo { get; set; }
        public List<string> idUserBcc { get; set; }

    }
}
