using BinusSchool.Common.Model;
using FluentValidation;

namespace BinusSchool.Common.Validators
{
    public class CopyToAcadyearValidator : AbstractValidator<CopyToAcadyear>
    {
        public CopyToAcadyearValidator()
        {
            RuleFor(x => x.IdAcadyearFrom)
                .NotEmpty()
                .NotEqual(x => x.IdAcadyearTo);

            RuleFor(x => x.IdAcadyearTo)
                .NotEmpty()
                .NotEqual(x => x.IdAcadyearFrom);
        }
    }
}