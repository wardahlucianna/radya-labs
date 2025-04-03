using System.Collections.Generic;

namespace BinusSchool.Data.Model.School.FnSchool.AnswerSet
{
    public class CopyListAnswerSetRequest
    {
        public string IdAcademicYear { get; set; }
        public List<ListAnswerSet> ListAnswerSets { get; set; }
    }

    public class ListAnswerSet
    {
        public string Id { get; set; }
        public string AnswerSetName { get; set; }
    }
}
