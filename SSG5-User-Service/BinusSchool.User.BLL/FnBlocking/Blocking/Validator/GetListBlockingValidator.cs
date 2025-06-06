﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using FluentValidation;

namespace BinusSchool.User.FnBlocking.Blocking.Validator
{
    public class GetListBlockingValidator : AbstractValidator<GetListBlockingRequest>
    {
        public GetListBlockingValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.IdStudent).NotEmpty();

        }
    }
}
