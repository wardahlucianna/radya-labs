﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular
{
    public class GetDetailStudentExtracurricularRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdLevel { get; set; }
        public string IdGrade { get; set; }
        public int Semester { get; set; }
        public string IdStudent { get; set; }
    }
}
