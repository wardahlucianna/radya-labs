using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnAuth.ImpersonateLogin;
using FluentValidation;

namespace BinusSchool.User.FnAuth.ImpersonateLogin.Validator
{
    public class ImpersonateLoginValidator : AbstractValidator<ImpersonateLoginRequest>
    {
        public ImpersonateLoginValidator()
        {
            RuleFor(x => x.ImpersonatorIdUser).NotEmpty();
            RuleFor(x => x.ImpersonatedUsername).NotEmpty();
            RuleFor(x => x.LoggedInIpAddress).NotEmpty();
        }
    }
}
