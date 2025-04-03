using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser;
using FluentValidation;

namespace BinusSchool.User.FnUser.LoginAs.Validator
{
    public class ImpersonateUserLoginAsValidator : AbstractValidator<ImpersonateUserLoginAsRequest>
    {
        public ImpersonateUserLoginAsValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();
            RuleFor(x => x.IdCurrentUser).NotEmpty();
            RuleFor(x => x.ImpresonatingUsername).NotEmpty();
            RuleFor(x => x.LoggedInIpAddress).NotEmpty();
        }
    }
}
