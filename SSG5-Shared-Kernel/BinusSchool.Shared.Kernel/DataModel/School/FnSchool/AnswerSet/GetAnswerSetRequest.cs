using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.School.FnSchool.AnswerSet
{
    public class GetAnswerSetRequest : CollectionRequest
    {
        public string IdAcademicYear { get; set; }
        public string AnswerSetName { get; set; }
    }
}
