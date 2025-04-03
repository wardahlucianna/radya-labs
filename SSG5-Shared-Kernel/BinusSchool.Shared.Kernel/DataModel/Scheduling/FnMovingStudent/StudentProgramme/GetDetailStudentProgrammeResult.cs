using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme
{
    public class GetDetailStudentProgrammeResult
    {
        public string id { get; set; }
        public string idStudent { get; set; }
        public string studentName { get; set; }
        public StudentProgrammeEnum programme { get; set; }
    }
}
