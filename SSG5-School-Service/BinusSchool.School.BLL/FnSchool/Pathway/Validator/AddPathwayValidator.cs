using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Pathway.Validator
{
    public class AddPathwayValidator : AbstractValidator<AddPathwayRequest>
    {
        public AddPathwayValidator()
        {
            RuleFor(x => x.IdAcadyear).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithName("Pathway Name");
        }
    }
}
