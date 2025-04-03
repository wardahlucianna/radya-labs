using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class SaveReadyDocumentRequest
    {
        public List<SaveReadyDocumentRequest_ChecklistStatus> ChecklistStatusList { get; set; }
    }
    public class SaveReadyDocumentRequest_ChecklistStatus
    {
        public string IdDocumentReqApplicantDetail { get; set; }
        public bool IsChecked { get; set; }
    }
}
