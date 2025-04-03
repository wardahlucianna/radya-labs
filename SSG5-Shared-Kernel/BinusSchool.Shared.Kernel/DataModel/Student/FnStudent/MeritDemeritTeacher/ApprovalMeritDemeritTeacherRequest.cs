using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher
{
    public class ApprovalMeritDemeritTeacherRequest
    {
        public string Id { get; set; }  
        public MeritDemeritCategory Category { get; set; }  
        public RequestType RequestType { get; set; }  
        public string IdLevel { get; set; }  
        public string IdGrade { get; set; }  
        public string IdUser { get; set; }  
        public string Note { get; set; }  
        public string IdHomeroomStudent { get; set; }  
        public string IdAcademicYear { get; set; }  
        public int Point { get; set; }  
    }
}
