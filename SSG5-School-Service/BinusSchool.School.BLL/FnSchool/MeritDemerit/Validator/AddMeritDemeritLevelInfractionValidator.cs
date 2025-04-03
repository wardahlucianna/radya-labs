using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using FluentValidation;

namespace BinusSchool.School.FnSchool.MeritDemerit.Validator
{
    internal class AddMeritDemeritLevelInfractionValidator : AbstractValidator<AddMeritDemeritLevelInfractionRequest>
    {
        public AddMeritDemeritLevelInfractionValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty().WithMessage("School code cannot empty");
        }
    }
}
