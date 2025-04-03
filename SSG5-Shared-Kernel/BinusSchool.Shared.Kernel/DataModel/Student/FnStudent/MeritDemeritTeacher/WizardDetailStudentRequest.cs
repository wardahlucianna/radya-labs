using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class WizardDetailStudentRequest :CollectionSchoolRequest
    {
        public string IdStudent { get; set; }   
        public string IdAcademicYear { get; set; }   
        public int? Semester { get; set; }   
    }
}
