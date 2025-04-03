using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.Blocking.Validator
{
    public class GetBlockingValidator : AbstractValidator<GetBlockingRequest>
    {
        public GetBlockingValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.IdStudent).NotEmpty();

            RuleFor(x => x.IdFeature).NotEmpty();

        }
    }
}
