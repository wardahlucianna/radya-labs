using BinusSchool.Data.Model.User.FnAuth.Logout;
using FluentValidation;

namespace BinusSchool.User.FnAuth.Logout.Logout
{
    public class LogoutValidator : AbstractValidator<LogoutRequest>
    {
        public LogoutValidator()
        {
            RuleFor(x => x.FirebaseToken)
                .NotEmpty()
                .MaximumLength(512);
        }
    }
}
