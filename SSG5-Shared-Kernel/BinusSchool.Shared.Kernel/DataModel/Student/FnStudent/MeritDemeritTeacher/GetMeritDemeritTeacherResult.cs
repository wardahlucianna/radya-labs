using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetMeritDemeritTeacherResult : CodeWithIdVm
    {
        public string IdHomeroomStudent { get; set; }    
        public string AcademicYear { get; set; }    
        public string IdAcademicYear { get; set; }    
        public string Semester { get; set; }    
        public string Level { get; set; }    
        public string IdLevel { get; set; }    
        public string Grade { get; set; }    
        public string IdGrade { get; set; }    
        public string Homeroom { get; set; }    
        public string IdStudent { get; set; }
        public string NameStudent { get; set; }
        public int Merit { get; set; }
        public int Demerit { get; set; }
        public string LevelOfInfraction { get; set; }
        public string Sanction { get; set; }
        public string IdHomeroom { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
