using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.HandbookManagement
{
    public class GetHandbookManagementDetailResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }

        public List<CodeWithIdVm> Levels { get; set; }

        public List<ViewForHandbookManagement> ViewFors { get; set; }

        public string Title { get; set; }

        public string Url { get; set; }

        public List<AttachmentHandbookManagement> Attachments { get; set; }
    }

    public class ViewForHandbookManagement
    {
        public int Index { get; set; }
        public string ViewFor { get; set; }
    }

}
