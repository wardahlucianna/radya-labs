using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection
{
    public class GetReflectionResult : CodeWithIdVm
    {
        public string Id { get; set; }
        public string ReflectionContent { get; set; }
        public string Term { get; set; }
        public string Semester { get; set; }
        public DateTime? DateReflection { get; set; }
        public List<ReflectionComment> ReflectionCommnet { get; set; }
        public List<ReflectionAttachment> ReflectionAttachments { get; set; }
    }

    public class ReflectionComment
    {
        public string Id { get; set; }
        public string ReflectionCommet { get; set; }
        public string User { get; set; }
        public DateTime? DateComment { get; set; }
        public bool IsShowButton { get; set; }
    }

    public class ReflectionAttachment
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string FileNameOriginal { get; set; }
    }
}
