using BinusSchool.Data.Model.School.FnSchool.Level;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Level.Validator
{
    public class AddLevelValidator : AbstractValidator<AddLevelRequest>
    {
        public AddLevelValidator()
        {
            RuleFor(x => x.IdAcadyear).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithName("Level Short Name");
            
            RuleFor(x => x.Description)
                .NotEmpty()
                .WithName("Level Name");

            RuleFor(x => x.OrderNumber)
                .GreaterThan(0)
                .WithMessage("Order number must be greater than 0");
        }
    }
}
