﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherComment
{
    public class ExportExcelTeacherCommentPerStudentRequest
    {
        public string IdSchool { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdStudent { set; get; }
    }
}
