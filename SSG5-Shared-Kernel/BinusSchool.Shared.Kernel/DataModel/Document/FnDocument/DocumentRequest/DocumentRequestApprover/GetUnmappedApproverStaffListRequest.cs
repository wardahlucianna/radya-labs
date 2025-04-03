using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestApprover
{
    public class GetUnmappedApproverStaffListRequest : CollectionRequest
    {
        public string IdSchool { get; set; }
    }
}
