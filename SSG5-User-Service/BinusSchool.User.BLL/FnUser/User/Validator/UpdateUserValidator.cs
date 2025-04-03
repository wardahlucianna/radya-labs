using BinusSchool.Data.Model.User.FnUser.User;
using FluentValidation;

namespace BinusSchool.User.FnUser.User.Validator
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserValidator()
        {
            Include(new AddUserValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
