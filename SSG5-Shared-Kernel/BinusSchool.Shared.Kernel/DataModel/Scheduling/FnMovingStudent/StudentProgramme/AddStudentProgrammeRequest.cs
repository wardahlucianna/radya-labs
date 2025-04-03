using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Scheduling.FnMovingStudent.StudentProgramme
{
    public class AddStudentProgrammeRequest
    {
        public StudentProgrammeEnum Programme { get; set; }
        public string idSchool { get; set; }
        public List<string> idstudent   { get; set; }
    }
}
