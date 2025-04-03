using System;
using System.Collections.Generic;
using BinusSchool.Common.Validators;
using BinusSchool.Data.Model.School.FnSchool.Floor;
using FluentValidation;

namespace BinusSchool.School.FnSchool.Floor.Validator
{
    public class SaveFloorValidator : AbstractValidator<SaveFloorRequest>
    {
        public SaveFloorValidator()
        {
            RuleFor(x => x.FloorName).NotEmpty();
            RuleFor(x => x.IdBuilding).NotEmpty();
            RuleFor(x => x.LockerTowerCodeName).NotNull();
            RuleFor(x => x.Description).NotEmpty();
        }
    }
}
