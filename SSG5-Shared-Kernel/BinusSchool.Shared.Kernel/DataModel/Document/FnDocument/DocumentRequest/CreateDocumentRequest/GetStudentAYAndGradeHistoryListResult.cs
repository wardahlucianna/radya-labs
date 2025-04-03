using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.CreateDocumentRequest
{
    public class GetStudentAYAndGradeHistoryListResult : ItemValueVm
    {
        public ItemValueVm AcademicYear { get; set; }
        public ItemValueVm Grade { get; set; }
        public string HomeroomName { get; set; }
    }
}
