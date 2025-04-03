using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class WizardStudentHandlerResult
    {
        public string IdHomeroomStudent { get; set; }
        public string IdGrade { get; set; }
        public bool IsPointSystem { get; set; }
        public bool IsDemeritSystem { get; set; }
        public int TotalMerit { get; set; }
        public int TotalDemerit { get; set; }
        public int CountMerit { get; set; }
        public int CountDemerit { get; set; }
        public string IdLevel { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string Level { get; set; }
        public int? Semester { get; set; }
        public string StudentName { get; set; }
        public string StudentId { get; set;}
    }
}
