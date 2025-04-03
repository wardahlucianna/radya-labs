using BinusSchool.Data.Model.User.FnUser.Register;
using FluentValidation;

namespace BinusSchool.User.FnUser.Register.Validator
{
    public class RegisterPushTokenValidator : AbstractValidator<RegisterPushTokenRequest>
    {
        public RegisterPushTokenValidator()
        {
            RuleFor(x => x.Platform).IsInEnum();

            RuleFor(x => x.FirebaseToken)
                .NotEmpty()
                .MaximumLength(512);
        }
    }
}
