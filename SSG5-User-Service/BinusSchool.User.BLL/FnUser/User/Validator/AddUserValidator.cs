using BinusSchool.Data.Model.User.FnUser.User;
using FluentValidation;
using System.Linq;

namespace BinusSchool.User.FnUser.User.Validator
{
    public class AddUserValidator : AbstractValidator<AddUserRequest>
    {
        public AddUserValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.DisplayName).NotEmpty();

            RuleFor(x => x.Roles)
                .NotNull()
                .Must(x => x.Count > 0)
                .Must(x => x.Where(y => y.IsDefaultUsername).Count() == 1)
                .WithMessage("1 role must be a default username, cannot more than 1")
                .Must(x => !x.Any(y => string.IsNullOrEmpty(y.Username)))
                .WithMessage("Username is required");
        }
    }
}
