using BinusSchool.Data.Model.User.FnUser.User;
using FluentValidation;

namespace BinusSchool.User.FnUser.User.Validator
{
    public class SetStatusUserValidator : AbstractValidator<SetStatusUserRequest>
    {
        public SetStatusUserValidator()
        {
            RuleFor(x => x.Ids)
                .NotNull();
        }
    }
}
