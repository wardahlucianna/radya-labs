using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class AddDocumentRequestOptionCategoryRequest
    {
        public string CategoryDescription { get; set; }
        public string IdDocumentReqFieldType { get; set; }
        public string IdSchool { get; set; }
        public List<AddDocumentRequestOptionCategoryRequest_Option> OptionList { get; set; }
    }

    public class AddDocumentRequestOptionCategoryRequest_Option
    {
        public string OptionDescription { get; set; }
    }
}
