using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.StudentStatus
{
    public class UpdateStudentStatusRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public int IdStudentStatus { get; set; }
        public int? IdStudentStatusSpecial { get; set; }
        public DateTime NewStatusStartDate { get; set; }
        public string Remarks { get; set; }
    }
}
