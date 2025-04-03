using BinusSchool.Common.Model;

namespace BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination
{
    public class GetSubjectCombinationRequest: CollectionRequest
    {
        public string IdAcadyear { get; set; }
        public string IdGrade { get; set; }
    }
}
