using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class ResetMeritDemeritPointRequest
    {
        public string IdSchool {get;set;}
        public string IdAcademicYear {get;set;}
        public string IdGrade {get;set;}
        public string CodeGrade {get;set;}
        public int Semester {get;set;}
    }
}
