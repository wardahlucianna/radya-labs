using System.Collections.Generic;
using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.AnswerSet
{
    public class AnswerSetDetailResult : CodeWithIdVm
    {
        public CodeWithIdVm AcademicYear { get; set; }
        public string AnswerSetName { get; set; }
        public List<AnswerSetOptionResult> AnswerSetOptions { get; set; }
    }

    public class AnswerSetOptionResult : CodeWithIdVm
    {
        public string OptionName { get; set; }
        public int Order { get; set; }
    }
}
