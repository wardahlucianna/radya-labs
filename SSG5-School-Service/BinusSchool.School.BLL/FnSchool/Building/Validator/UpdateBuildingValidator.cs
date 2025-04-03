using System;
using System.Collections.Generic;
using BinusSchool.Data.Model.School.FnSchool.Building;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Building.Validator
{
    public class UpdateBuildingValidator : AbstractValidator<UpdateBuildingRequest>
    {
        public UpdateBuildingValidator()
        {
            Include(new AddBuildingValidator());

            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
