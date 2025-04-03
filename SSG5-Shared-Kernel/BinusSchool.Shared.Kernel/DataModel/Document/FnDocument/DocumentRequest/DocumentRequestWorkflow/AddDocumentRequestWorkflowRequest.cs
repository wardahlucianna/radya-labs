using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow
{
    public class AddDocumentRequestWorkflowRequest
    {
        public string IdDocumentReqApplicant { get; set; }
        public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
        public string IdBinusianStaff { get; set; }
        public string Remarks { get; set; }
    }
}
