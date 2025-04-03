using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestWorkflow
{
    public class GetDocumentRequestStatusWorkflowListResult : ItemValueVm
    {
        public DocumentRequestStatusWorkflow IdDocumentReqStatusWorkflow { get; set; }
    }
}
