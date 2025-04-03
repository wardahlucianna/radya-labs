using BinusSchool.Data.Model.User.FnUser.User;
using FluentValidation;

namespace BinusSchool.User.FnUser.User.Validator
{
    public class AddUserSupervisorForExperienceValidator : AbstractValidator<AddUserSupervisorForExperienceRequest>
    {
        public AddUserSupervisorForExperienceValidator()
        {
            RuleFor(x => x.IdSchool)
                .NotEmpty()
                .WithMessage("Id School cannot be empty");

            RuleFor(x => x.IsActiveDirectory)
                .NotNull()
                .WithMessage("Is Active Directory cannot be null");

            RuleFor(x => x.Email)
                .NotNull()
                .WithMessage("Email cannot be null");

            RuleFor(x => x.IdRole)
                .NotNull()
                .WithMessage("Id Role cannot be null");
        }
    }
}
