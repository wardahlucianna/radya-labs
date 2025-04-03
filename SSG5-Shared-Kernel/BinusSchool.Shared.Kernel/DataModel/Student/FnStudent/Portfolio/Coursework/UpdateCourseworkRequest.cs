using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework
{
    public class UpdateCourseworkRequest
    {
        public string Id { get; set; }
        public string IdAcademicYear { get; set; }
        public string IdStudent { get; set; }
        public string IdUOI { get; set; }
        public string Content { get; set; }
        public bool NotifyParentStudent { get; set; }
        public int Type { get; set; }
        public int Semester { get; set; }
        public List<CourseworkAttachment> Attachments { get; set; }
    }
}
