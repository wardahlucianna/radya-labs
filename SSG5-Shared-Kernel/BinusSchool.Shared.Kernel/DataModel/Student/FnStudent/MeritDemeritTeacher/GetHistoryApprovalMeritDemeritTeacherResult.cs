using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetHistoryApprovalMeritDemeritTeacherResult : CollectionSchoolRequest
    {
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string IdStudent { get; set; }
        public string NameStudent { get; set; }
        public string LevelOfInfraction { get; set; }
        public string NameDecipline { get; set; }
        public int Point { get; set; }
        public string Note { get; set; }
        public string CreateBy { get; set; }
        public string Approver1 { get; set; }
        public string NoteApprover1 { get; set; }
        public string Approver2 { get; set; }
        public string NoteApprover2 { get; set; }
        public string Approver3 { get; set; }
        public string NoteApprover3 { get; set; }
        public RequestType RequestType { get; set; }
        public string RequestReason { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
       
    }
}
