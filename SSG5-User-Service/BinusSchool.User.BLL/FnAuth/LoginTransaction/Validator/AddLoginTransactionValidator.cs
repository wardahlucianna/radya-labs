using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnAuth.LoginTransaction;
using FluentValidation;

namespace BinusSchool.User.FnAuth.LoginTransaction.Validator
{
    public class AddLoginTransactionValidator : AbstractValidator<AddLoginTransactionRequest>
    {
        public AddLoginTransactionValidator()
        {
            RuleFor(x => x.IdUser)
                .NotEmpty();

            RuleFor(x => x.Action)
                .Must(x => x.ToUpper() == "LOGIN" || x.ToUpper() == "LOGOUT")
                .NotEmpty();
        }
    }
}
