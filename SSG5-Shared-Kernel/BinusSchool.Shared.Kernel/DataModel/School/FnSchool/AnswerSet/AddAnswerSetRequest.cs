using System.Collections.Generic;

namespace BinusSchool.Data.Model.School.FnSchool.AnswerSet
{
    public class AddAnswerSetRequest
    {
        public string IdAcademicYear { get; set; }
        public string AnswerSetName { get; set; }
        public List<AnswerSetOptionRequest> AnswerSetOptions { get; set; }
    }

    public class AnswerSetOptionRequest
    {
        public string OptionName { get; set; }
    }
}
