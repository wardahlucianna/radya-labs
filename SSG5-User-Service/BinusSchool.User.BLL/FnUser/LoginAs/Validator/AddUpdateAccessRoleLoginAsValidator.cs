using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ManageAccessRoleLoginAs;
using FluentValidation;

namespace BinusSchool.User.FnUser.LoginAs.Validator
{
    public class AddUpdateAccessRoleLoginAsValidator : AbstractValidator<AddUpdateAccessRoleLoginAsRequest>
    {
        public AddUpdateAccessRoleLoginAsValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdRole).NotEmpty();
        }
    }
}
