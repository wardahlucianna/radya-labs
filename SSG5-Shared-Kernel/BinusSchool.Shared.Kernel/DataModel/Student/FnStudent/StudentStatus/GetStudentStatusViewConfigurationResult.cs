using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentStatus
{
    public class GetStudentStatusViewConfigurationResult
    {
        public bool IsEditable { get; set; }
        public bool EnableCreateStudentRecord { get; set; }
        public bool EnableGenerateStudentStatus { get; set; }
    }
}
