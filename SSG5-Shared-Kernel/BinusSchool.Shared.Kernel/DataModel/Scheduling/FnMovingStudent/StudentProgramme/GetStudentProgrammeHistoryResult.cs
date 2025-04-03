using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme
{
    public class GetStudentProgrammeHistoryResult : CodeWithIdVm
    {
        public string idStudent { get; set; }
        public string studentName { get; set; }
        public string programmeNew { get; set; }
        public string programmeOld { get; set; }
        public DateTime effectiveDate { get; set; }
    }
}
