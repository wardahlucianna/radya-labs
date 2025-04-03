using BinusSchool.Data.Model.User.FnUser.Role;
using FluentValidation;

namespace BinusSchool.User.FnUser.Role.Validator
{
    public class UpdateRoleValidator : AbstractValidator<UpdateRoleRequest>
    {
        public UpdateRoleValidator()
        {
            Include(new AddRoleValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
