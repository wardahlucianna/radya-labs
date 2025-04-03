using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme
{
    public class GetStudentProgrammeStudentResult : CodeWithIdVm
    {
        public string idStudent { get; set; }
        public string studentName { get; set; }
        public string homeroom { get; set; }
    }
}
