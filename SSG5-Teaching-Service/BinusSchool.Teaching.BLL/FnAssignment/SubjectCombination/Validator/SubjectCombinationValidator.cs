using BinusSchool.Data.Model.Teaching.FnAssignment.SubjectCombination;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.SubjectCombination.Validator
{
    public class SubjectCombinationValidator : AbstractValidator<AddSubjectCombination>
    {
        public SubjectCombinationValidator()
        {
            RuleForEach(x => x.SubjectCombination).NotNull();
        }
    }
}
