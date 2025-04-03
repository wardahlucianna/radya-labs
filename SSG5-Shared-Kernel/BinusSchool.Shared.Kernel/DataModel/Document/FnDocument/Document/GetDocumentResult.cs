using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.Document
{
    public class GetDocumentResult : CodeWithIdVm
    {
        public string Acadyear { get; set; }
        public string Semester { get; set; }
        public string Term { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Subject { get; set; }
        public string Status { get; set; }
        public string ForNextRole { get; set; }
        public DateTime? CraeteDate { get; set; }
    }
}
