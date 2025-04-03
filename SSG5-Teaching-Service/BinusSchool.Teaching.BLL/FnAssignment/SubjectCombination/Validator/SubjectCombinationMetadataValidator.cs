using BinusSchool.Common.Model;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.SubjectCombination.Validator
{
    public class SubjectCombinationMetadataValidator : AbstractValidator<IdCollection>
    {
        public SubjectCombinationMetadataValidator()
        {
            RuleFor(x => x.Ids).NotNull();
        }
    }
}
