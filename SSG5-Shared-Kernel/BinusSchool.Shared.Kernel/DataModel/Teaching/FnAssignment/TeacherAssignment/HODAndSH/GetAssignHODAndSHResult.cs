using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.HODAndSH
{
    public class GetAssignHODAndSHResult : IItemValueVm
    {
        public string Id { get; set; }
        public ItemValueVm AcademicYear { get; set; }
        public string Level { get; set; }
        public ItemValueVm Department { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<string> DeletedIds { get; set; }
    }
}
