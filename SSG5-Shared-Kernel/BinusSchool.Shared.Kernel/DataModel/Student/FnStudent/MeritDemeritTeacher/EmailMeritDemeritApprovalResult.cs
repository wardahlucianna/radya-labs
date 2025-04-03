using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class EmailMeritDemeritApprovalResult
    {
        public string Id { get; set; }
        public string IdStudent { get; set; }
        public string StudentName { get; set; }
        public string Category { get; set; }
        public MeritDemeritCategory CategoryEnum { get; set; }
        public string DisciplineName { get; set; }
        public string Point { get; set; }
        public string Note { get; set; }
        public string TeacherName { get; set; }
        public string TeacherId { get; set; }
        public string CreateDate { get; set; }
        public string SchoolName { get; set; }
        public string LevelOfInfraction { get; set; }
        public string RequestType { get; set; }
        public string Status { get; set; }
        public string StudentEmail { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdGrade { get; set; }
        public string IdLevel { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string Link { get; set; }
    }
}
