using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow
{
    public class AddDocumentRequestWorkflowResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string IdDocumentReqStatusTrackingHistory { get; set; }
    }
}
