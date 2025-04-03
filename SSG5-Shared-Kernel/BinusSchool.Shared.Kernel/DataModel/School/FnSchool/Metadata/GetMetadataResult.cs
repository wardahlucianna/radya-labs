using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.Metadata
{
    public class GetMetadataResult
    {
        public IEnumerable<ItemValueVm> Acadyears { get; set; }
        public IEnumerable<ItemValueVm> Levels { get; set; }
        public IEnumerable<ItemValueVm> Grades { get; set; }
        public IEnumerable<ItemValueVm> Terms { get; set; }
        public IEnumerable<ItemValueVm> Subjects { get; set; }
        public IEnumerable<ItemValueVm> Classrooms { get; set; }
        public IEnumerable<ItemValueVm> Departments { get; set; }
        public IEnumerable<ItemValueVm> Streamings { get; set; }
        public IEnumerable<ItemValueVm> Buldings { get; set; }
        public IEnumerable<ItemValueVm> Venues { get; set; }
        public IEnumerable<ItemValueVm> Divisions { get; set; }
        public IEnumerable<ItemValueVm> SubjectCombinations { get; set; }
    }
}
