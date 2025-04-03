using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection
{
    public class UpdateReflectionRequest
    {
        public UpdateReflectionRequest()
        {
            Attachments = new List<ReflectionAttachmentRequest>();
        }
        public string Id { get; set; }
        public string IdStudent { get; set; }
        public string Content { get; set; }
        public string IdAcademicYear { get; set; }
        public int Semester { get; set; }
        public List<ReflectionAttachmentRequest> Attachments { get; set; }
    }
}
