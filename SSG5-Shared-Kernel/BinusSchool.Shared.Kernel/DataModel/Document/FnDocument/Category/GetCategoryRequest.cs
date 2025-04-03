using System;

namespace BinusSchool.Data.Model.Document.FnDocument.Category
{
    public class GetCategoryRequest : CollectionSchoolRequest
    {
        public GetCategoryRequest()
        {
            DocumentType = string.Empty;
        }
        
        public string DocumentType { get; set; }
    }
}