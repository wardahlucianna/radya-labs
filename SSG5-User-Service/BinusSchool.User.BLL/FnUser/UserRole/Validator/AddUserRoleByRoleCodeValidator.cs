using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnUser.UserRole;
using FluentValidation;

namespace BinusSchool.User.FnUser.UserRole.Validator
{   
    public class AddUserRoleByRoleCodeValidator : AbstractValidator<AddUserRoleByRoleCodeRequest>
    {
        public AddUserRoleByRoleCodeValidator()
        {          
            RuleFor(x => x.IdUser).NotEmpty();
            RuleFor(x => x.RoleCode).NotEmpty();
        }
    }
}
