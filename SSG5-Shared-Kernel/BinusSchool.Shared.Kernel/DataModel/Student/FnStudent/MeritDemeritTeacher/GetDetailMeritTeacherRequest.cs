using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailMeritTeacherRequest : CollectionSchoolRequest
    {
        public string IdHomeroomStudent { get; set;}
        public string IdUser { get; set;}
        public string IdHomeroom { get; set;}
        public string IdGrade { get; set;}
        public string IdAcademicYear { get; set;}
        public string IdLevel { get; set;}
    }
}
