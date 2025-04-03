using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class WizardTeacherResult :CollectionSchoolRequest
    {
        public string IdHomeroomStudent { get; set; }
        public string IdStudent { get; set; }
        public string NameStudent { get; set; }
        public int Demerit { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string UserUpdate { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdHomeroom { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
    }
}
