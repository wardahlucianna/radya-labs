using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetDetailApprovalMeritDemeritResult
    {
        public string Id { get; set; }
        public string AcademicYear { get; set; }
        public string RequestType { get; set; }
        public string Semester { get; set; }
        public string CreateBy { get; set; }
        public string Student { get; set; }
        public string Homeroom { get; set; }
        public string Category { get; set; }
        public string LevelOfInfraction { get; set; }
        public string DisciplineName { get; set; }
        public string Point { get; set; }
        public string Note { get; set; }
        public string UserApproval1 { get; set; }
        public string UserApproval2 { get; set; }
        public string UserApproval3 { get; set; }
        public string NoteApproval1 { get; set; }
        public string NoteApproval2 { get; set; }
        public string NoteApproval3 { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public bool IsShowButtonApproval { get; set; }
    }
}
