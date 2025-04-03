using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using FluentValidation;

namespace BinusSchool.User.FnAuth.UserPassword.Validator
{
    public class UserPasswordValidator : AbstractValidator<UserPasswordRequest>
    {
        public UserPasswordValidator()
        {
            RuleFor(x => x.UserName).NotEmpty();

            RuleFor(x => x.Password).NotEmpty();
        }
    }
}