using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetStudentAYAndGradeHistoryListRequest : CollectionRequest
    {
        public string IdStudent { get; set; }
    }
}
