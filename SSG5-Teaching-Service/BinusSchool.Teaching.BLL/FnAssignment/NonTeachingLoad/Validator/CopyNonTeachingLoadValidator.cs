using BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad;
using FluentValidation;

namespace BinusSchool.Teaching.FnAssignment.NonTeachingLoad.Validator
{
    public class CopyNonTeachingLoadValidator : AbstractValidator<CopyNonTeachingLoadRequest>
    {
        public CopyNonTeachingLoadValidator()
        {
            RuleFor(x => x.IdAcadyearTarget).NotNull();
            RuleFor(x => x.Ids).NotEmpty();
        }
    }
}
