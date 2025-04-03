using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnUser.Register;
using FluentValidation;

namespace BinusSchool.User.FnUser.Register.Validator
{
    public class GetFirebaseTokenRequestValidator : AbstractValidator<GetFirebaseTokenRequest>
    {
        public GetFirebaseTokenRequestValidator()
        {
            //RuleFor(x => x.Platform).IsInEnum();

            //RuleFor(x => x.FirebaseToken)
            //    .NotEmpty()
            //    .MaximumLength(512);
        }
    }
}
