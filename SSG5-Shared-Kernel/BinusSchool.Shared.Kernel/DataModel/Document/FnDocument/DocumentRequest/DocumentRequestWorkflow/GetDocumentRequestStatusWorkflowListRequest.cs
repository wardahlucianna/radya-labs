using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow
{
    public class GetDocumentRequestStatusWorkflowListRequest : CollectionRequest
    {
        public bool IsFromParent { get; set; }
    }
}
