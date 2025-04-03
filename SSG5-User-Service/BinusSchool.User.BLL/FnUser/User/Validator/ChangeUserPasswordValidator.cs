using BinusSchool.Data.Model.User.FnUser.User;
using FluentValidation;

namespace BinusSchool.User.FnUser.User.Validator
{
    public class ChangeUserPasswordValidator : AbstractValidator<ChangeUserPasswordRequest>
    {
        public ChangeUserPasswordValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User id cannot be empty");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Please fill your new password");
        }
    }
}
