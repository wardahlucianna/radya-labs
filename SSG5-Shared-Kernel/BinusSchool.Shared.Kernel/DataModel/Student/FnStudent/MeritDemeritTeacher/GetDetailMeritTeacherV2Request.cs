using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailMeritTeacherV2Request : CollectionSchoolRequest
    {
        public string IdHomeroomStudent { get; set; }
        public string IdUser { get; set; }
        public string IdHomeroom { get; set; }
        public string IdGrade { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public EntryMeritStudentType? Type { get; set; }
        public int Semester { get; set; }
    }
}
