using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser;
using FluentValidation;

namespace BinusSchool.User.FnUser.LoginAs.Validator
{
    public class CheckUserNamePasswordImpersonateUserLoginAsValidator : AbstractValidator<CheckUserNamePasswordImpersonateUserLoginAsRequest>
    {
        public CheckUserNamePasswordImpersonateUserLoginAsValidator()
        {
            RuleFor(x => x.ImpersonatedUsername).NotEmpty();
            //RuleFor(x => x.Password).NotEmpty();
        }
    }
}
