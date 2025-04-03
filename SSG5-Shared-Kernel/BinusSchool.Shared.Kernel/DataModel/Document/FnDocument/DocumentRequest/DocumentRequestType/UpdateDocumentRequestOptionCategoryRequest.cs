using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class UpdateDocumentRequestOptionCategoryRequest
    {
        public string IdDocumentReqOptionCategory { get; set; }
        public string CategoryDescription { get; set; }
        public List<UpdateDocumentRequestOptionCategoryRequest_Option> NewOptionList { get; set; }
        public List<string> IdDocumentReqOptionDeletedList { get; set; }
    }

    public class UpdateDocumentRequestOptionCategoryRequest_Option
    {
        public string OptionDescription { get; set; }
    }
}
