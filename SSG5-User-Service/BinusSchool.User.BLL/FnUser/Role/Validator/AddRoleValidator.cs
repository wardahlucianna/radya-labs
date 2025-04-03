using BinusSchool.Data.Model.User.FnUser.Role;
using BinusSchool.User.FnUser.Utils;
using FluentValidation;

namespace BinusSchool.User.FnUser.Role.Validator
{
    public class AddRoleValidator : AbstractValidator<AddRoleRequest>
    {
        public AddRoleValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.IdRoleGroup).NotEmpty();

            RuleFor(x => x.Code).NotEmpty();

            RuleFor(x => x.Description).NotEmpty();

            When(x => x.IsArrangeUsernameFormat, () => {

                RuleFor(x => x.UsernameFormat)
                    .Must(x => x.Validate())
                    .WithMessage("Username format is not valid");
            });
        }
    }
}
