using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnAuth.ImpersonateLogin;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using FluentValidation;

namespace BinusSchool.User.FnAuth.ImpersonateLogin.Validator
{
    public class MCB01X7UserPasswordValidator : AbstractValidator<MCB01X7UserPasswordRequest>
    {
        public MCB01X7UserPasswordValidator()
        {
            RuleFor(x => x.UserName).NotEmpty();

            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
