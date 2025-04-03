using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class SaveFinishAndCollectReqDocumentRequest
    {
        public string IdDocumentReqApplicant { get; set; }
        public DateTime? ScheduleCollectionDateStart { get; set; }
        public DateTime? ScheduleCollectionDateEnd { get; set; }
        public string IdVenue { get; set; }
        public string Remarks { get; set; }
        public string CollectedBy { get; set; }
        public DateTime? CollectedDate { get; set; }
    }
}
