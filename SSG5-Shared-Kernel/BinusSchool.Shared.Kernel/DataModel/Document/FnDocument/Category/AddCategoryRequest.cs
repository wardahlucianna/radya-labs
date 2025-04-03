using System;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.Category
{

    public class AddCategoryRequest : CodeVm
    {
        public string IdDocumentCategory { get; set; }
        public string IdSchoolDocumentType { get; set; }
    }
}