using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class GetOptionsByOptionCategoryResult
    {
        public string IdDocumentReqOptionCategory { get; set; }
        public string CategoryName { get; set; }
        public bool IsDefaultImportData { get; set; }
        public List<GetOptionsByOptionCategoryResult_Option> OptionList { get; set; }
    }

    public class GetOptionsByOptionCategoryResult_Option
    {
        public string IdDocumentReqOption { get; set; }
        public string OptionDescription { get; set; }
        public bool IsImportOption { get; set; }
        public bool CanDelete { get; set; }
    }
}
