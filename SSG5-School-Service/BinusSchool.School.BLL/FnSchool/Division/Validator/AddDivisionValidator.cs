using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.Division;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Divisions.Validator
{
    public class AddDivisionValidator : AbstractValidator<AddDivisionRequest>
    {
        public AddDivisionValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithName("Division Short Name");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithName("Division Name");
        }
    }
}
