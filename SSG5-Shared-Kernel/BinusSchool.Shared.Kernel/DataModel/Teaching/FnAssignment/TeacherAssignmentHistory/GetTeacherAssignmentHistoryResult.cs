using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignmentHistory
{
    public class GetTeacherAssignmentHistoryResult
    {
        public string IdUser { get; set; }
        public string TeacherName { get; set; }
        public List<GetTeacherAssignmentHistoryResult_Assignment> Assignments { get; set; }
    }

    public class GetTeacherAssignmentHistoryResult_Assignment
    {
        public string TeacherPosition { get; set; }
        public string AcademicYear { get; set; }
        public string SchoolName { get; set; }
        public string Grade { get; set; }
        public string Level { get; set; }
        public string Subject { get; set; }
        public string Department { get; set; }
        public string Identifier { get; set; } = "N/A";
        public string AcademicUnit { get; set; } = "N/A";
        public string AcademicRank { get; set; } = "N/A";
        public string TrackType { get; set; } = "N/A";
    }
}
