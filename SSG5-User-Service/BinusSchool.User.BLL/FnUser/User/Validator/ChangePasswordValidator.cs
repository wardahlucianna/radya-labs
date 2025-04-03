using BinusSchool.Data.Model.User.FnUser.User;
using FluentValidation;

namespace BinusSchool.User.FnUser.User.Validator
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.RequestCode)
                .NotEmpty()
                .WithMessage("Request code is empty");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Please fill your new password");
        }
    }
}
