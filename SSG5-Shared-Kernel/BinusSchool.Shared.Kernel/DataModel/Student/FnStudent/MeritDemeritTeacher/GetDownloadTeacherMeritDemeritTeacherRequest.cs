﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDownloadTeacherMeritDemeritTeacherRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int? Semester { get; set; }
        public string IdHomeroom { get; set; }
        public string Search { get; set; }
    }
}
