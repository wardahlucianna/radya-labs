using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetDefaultPICListResult : ItemValueVm
    {
        public string PICName { get; set; }
        public string PICEmail { get; set; }
    }
}
