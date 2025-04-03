using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetParentWithRoleByStudentResult : ItemValueVm
    {
        public string ParentName { get; set; }
        public ItemValueVm ParentRole { get; set; }
    }
}
