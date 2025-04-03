using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover
{
    public class GetApproverListBySchoolResult
    {
        public string IdDocumentReqApprover { get; set; }
        public NameValueVm Approver { get; set; }
        public string BinusianEmail { get; set; }
        public NameValueVm InsertedBy { get; set; }
        public DateTime? InsertedTime { get; set; }
        public bool CanRemove { get; set; }
    }
}
