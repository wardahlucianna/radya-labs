using System;
using System.Collections.Generic;

namespace BinusSchool.Data.Model.Student.FnStudent.EmergencyAttendance
{
    public class SubmitRequest
    {
        public DateTime Date { get; set; }
        public List<string> IdStudents { get; set; }
    }
}
