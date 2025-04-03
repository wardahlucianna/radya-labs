using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetMeritDemeritTeacherRequest : CollectionSchoolRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
        public string IdHomeroom { get; set; }
        public string ScoreSetting { get; set; }
        public string IdUser { get; set; }
        public string PositionCode { get; set; }
    }
}
