using System.Collections.Generic;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination
{
    public class AddSubjectCombination
    {
       public List<SubjectCombination> SubjectCombination { get; set; }
    }

    public class SubjectCombination 
    {
        public string Id { get; set; }
        public string IdGradeClass { get; set; }
        public string IdSubject { get; set; }
        public bool? IsDelete { get; set; }
    }

    public class PostSubjectCombination
    {
        public List<string> ExistingData { get; set; }
        public List<string> Item { get; set; }
    }

}

