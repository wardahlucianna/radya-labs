using System.Collections.Generic;

namespace BinusSchool.Data.Model.School.FnSchool.Metadata
{
    public class GetMetadataRequest
    {
        public IEnumerable<string> Acadyears { get; set; }
        public IEnumerable<string> Levels { get; set; }
        public IEnumerable<string> Grades { get; set; }
        public IEnumerable<string> Terms { get; set; }
        public IEnumerable<string> Subjects { get; set; }
        public IEnumerable<string> Classrooms { get; set; }
        public IEnumerable<string> Departments { get; set; }
        public IEnumerable<string> Streamings { get; set; }
        public IEnumerable<string> Buildings { get; set; }
        public IEnumerable<string> Venues { get; set; }
        public IEnumerable<string> Divisions { get; set; }
        public IEnumerable<string> SubjectCombinations { get; set; }
    }
}
