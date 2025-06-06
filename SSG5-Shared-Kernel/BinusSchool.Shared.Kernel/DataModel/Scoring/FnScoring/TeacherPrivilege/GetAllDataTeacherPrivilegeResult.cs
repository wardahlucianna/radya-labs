﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege
{
    public class GetAllDataTeacherPrivilegeResult
    {
        public string IdStudent { set; get; }
        public string IdLevel { set; get; }
        public string IdGrade { set; get; }
        public string IdHomeroom { set; get; }
        public string IdLesson { set; get; }
        public string ClassIdGenerated { set; get; }
        public string IdSubject { set; get; }
        public string SubjectID { set; get; }
        public string IdSubjectType { set; get; }
        public string IdSubjectLevel { set; get; }
        public string IdDepartment { set; get; }
        public int Semester { set; get; }
    }
}
