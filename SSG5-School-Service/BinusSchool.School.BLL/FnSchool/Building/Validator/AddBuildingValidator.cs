using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.Building;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Building.Validator
{
    public class AddBuildingValidator : AbstractValidator<AddBuildingRequest>
    {
        public AddBuildingValidator()
        {
            RuleFor(x => x.IdSchool).NotEmpty();

            RuleFor(x => x.Code)
                .NotEmpty()
                .WithName("Building Name");
        }
    }
}
