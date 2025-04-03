using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.Template
{
    public class GetTemplateResult : CodeWithIdVm
    {
        public string Acadyear { get; set; }
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Subject { get; set; }
    }
}
