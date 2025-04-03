using System;
using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework
{
    public class GetListCourseworkResult : CodeWithIdVm
    {
        public string TeacherName { get; set; }
        public string BinusianID { get; set; }
        public DateTime? Date { get; set; }
        public bool StatusUpdated { get; set; }
        public string? UOIName { get; set; }
        public string Content { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public List<CourseworkSeenBy> SeenBy { get; set; }
        public List<CourseworkAttachment> Attachments { get; set; }
        public List<CourseworkComments> Comments { get; set; }
    }

    public class CourseworkSeenBy
    {
        public string Id { get; set; }
        public string IdUserSeen { get; set; }
        public string Fullname { get; set; }
    }

    public class CourseworkComments
    {
        public string Id { get; set; }
        public string BinusianID { get; set; }
        public string Fullname { get; set; }
        public DateTime? Date { get; set; }
        public string Comment { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}