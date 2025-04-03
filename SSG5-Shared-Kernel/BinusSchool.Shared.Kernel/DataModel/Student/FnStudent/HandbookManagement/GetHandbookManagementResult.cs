using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Student.FnStudent.HandbookManagement
{
    public class GetHandbookManagementResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }

        public string Levels { get; set; }

        public string ViewFors { get; set; }

        public string Title { get; set; }

        public string Date { get; set; }

    }

    public class GetHandbookManagementQueryResult : ItemValueVm
    {
        public CodeWithIdVm AcademicYear { get; set; }

        public List<CodeWithIdVm> Levels { get; set; }

        public List<ViewForHandbookManagement> ViewFors { get; set; }

        public string Title { get; set; }

        public string Date { get; set; }

    }
}
