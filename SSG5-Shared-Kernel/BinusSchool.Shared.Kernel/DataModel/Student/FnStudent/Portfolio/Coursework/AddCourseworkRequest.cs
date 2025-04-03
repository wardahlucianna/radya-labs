using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework
{
    public class AddCourseworkRequest
    {
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string IdUOI { get; set; }
        public string Content { get; set; }
        public bool NotifyParentStudent { get; set; }
        public int Type { get; set; }
        public string IdUser { get; set; }
        public int Semester { get; set; }
        public List<CourseworkAttachment> Attachments { get; set; }
    }

    public class CourseworkAttachment
    {
        public string Id { get; set; }
        public string IdCourseworkAnecdotalStudent { get; set; }
        public string Url { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public int Filesize { get; set; }
        public string OriginalFilename { get; set; }
    }
}
