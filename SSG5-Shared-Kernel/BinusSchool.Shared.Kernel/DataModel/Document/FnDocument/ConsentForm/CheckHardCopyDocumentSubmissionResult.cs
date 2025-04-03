using System;
using System.Collections.Generic;
using System.Text;

namespace BinusSchool.Data.Model.Document.FnDocument.ConsentForm
{
    public class CheckHardCopyDocumentSubmissionResult
    {
        public bool IsDone { get; set; }
        public bool NeedHardCopy { get; set; }
        public DateTime? HardCopySubmissionDate { get; set; }
    }
}
