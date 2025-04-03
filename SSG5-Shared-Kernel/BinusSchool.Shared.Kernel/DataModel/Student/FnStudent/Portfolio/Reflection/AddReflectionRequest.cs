using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection
{
    public class AddReflectionRequest
    {
        public AddReflectionRequest()
        {
            Attachments = new List<ReflectionAttachmentRequest>();
        }
        public string IdStudent { get; set; }
        public string Content { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public List<ReflectionAttachmentRequest> Attachments { get; set; }
    }

    public class ReflectionAttachmentRequest
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string FileNameOriginal { get; set; }
    }
}
