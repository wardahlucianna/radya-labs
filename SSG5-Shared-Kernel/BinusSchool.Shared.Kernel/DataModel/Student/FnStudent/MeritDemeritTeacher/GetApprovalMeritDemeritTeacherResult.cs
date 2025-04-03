using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class GetApprovalMeritDemeritTeacherResult : CodeWithIdVm
    {
        public string Id { get; set; }
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
        public string IdStudent { get; set; }
        public string NameStudent { get; set; }
        public string Category { get; set; }
        public string LevelOfInfraction { get; set; }
        public string NameDecipline { get; set; }
        public int Point { get; set; }
        public string Note { get; set; }
        public string CreateBy { get; set; }
        public RequestType RequestType { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string IdLevel { get; set; }
        public string IdHomeroomStudent { get; set; }
        public string IdAcademicYear { get; set; }
        public bool IsShowButtonApproval { get; set; }
    }
}
