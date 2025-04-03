using BinusSchool.Data.Model.User.FnAuth.UserActiveDirectory;
using FluentValidation;

namespace BinusSchool.User.FnAuth.UserActiveDirectory.Validator
{
    public class UserActiveDirectoryValidator : AbstractValidator<UserActiveDirectoryRequest>
    {
        public UserActiveDirectoryValidator()
        {
            RuleFor(x => x.Token);
        }
    }
}
