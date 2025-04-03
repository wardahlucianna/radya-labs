using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme
{
    public class GetStudentProgrammeResult : CodeWithIdVm
    {
        public string idStudent { get; set; }
        public string studentName { get; set; }
        public string homeroom { get; set; }
        public string programme { get; set; }
        public DateTime? lastSaved { get; set; }
    }
}
