using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using NPOI.OpenXmlFormats.Dml;

namespace BinusSchool.Data.Model.Student.FnStudent.Portfolio.Reflection
{
    public class GetDetailReflectionResult
    {
        public string Id { get; set; }
        public string IdStudent { get; set; }
        public string Content { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public int Semester { get; set; }
        public List<GetReflectionAttachment> Attachments { get; set; }
    }

    public class GetReflectionAttachment
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public decimal FileSize { get; set; }
        public string FileNameOriginal { get; set; }
    }
}
