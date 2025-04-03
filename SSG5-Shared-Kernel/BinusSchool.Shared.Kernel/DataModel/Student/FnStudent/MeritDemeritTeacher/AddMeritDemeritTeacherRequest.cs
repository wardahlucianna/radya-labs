using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class AddMeritDemeritTeacherRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public int Point { get; set; }
        public DateTime Date { get; set; }
        public MeritDemeritCategory Category { get; set; }
        public string IdLevelInfraction { get; set; }
        public string IdMeritDemeritMapping { get; set; }
        public string NameMeritDemeritMapping { get; set; }
        //public string IdUser { get; set; }
        public List<MeritDemeritTeacher> MeritDemeritTeacher { get; set; }
    }

    public class MeritDemeritTeacher
    {
        public string IdHomeroomStudent { get; set; }
        public string Note { get; set; }
    }
}
